using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using TimeSight.Models;

namespace TimeSight.Services;

public class MicrosoftGraphService(Supabase.Client supabase, IHttpClientFactory httpClientFactory, IJSRuntime js)
{
    private static string CalendarStorageKey(Guid workspaceId) => $"timesight.outlook-calendar-id.{workspaceId}";
    private static string EventMapStorageKey(Guid workspaceId) => $"timesight.outlook-event-map.{workspaceId}";
    private const string AccessTokenStorageKey = "timesight.ms-access-token";
    private const string RefreshTokenStorageKey = "timesight.ms-refresh-token";

    // Supabase only returns the Microsoft provider token on the initial OAuth
    // exchange and drops it on every subsequent session refresh, so it's captured
    // separately in localStorage at login time (see Authentication.razor) instead
    // of being read from supabase.Auth.CurrentSession here.
    private async Task<string?> GetAccessTokenAsync() =>
        await js.InvokeAsync<string?>("localStorage.getItem", AccessTokenStorageKey);

    private async Task<HttpRequestMessage> BuildRequestAsync(HttpMethod method, string relativeUrl, object? body = null)
    {
        var req = new HttpRequestMessage(method, relativeUrl);
        var token = await GetAccessTokenAsync();
        if (token is not null)
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            req.Content = JsonContent.Create(body);
        return req;
    }

    // The Microsoft access token is short-lived (~1 hour) and Supabase never refreshes
    // it on our behalf, so any call can come back 401 once it expires. When that happens
    // we exchange the stored provider_refresh_token for a new access token (via the
    // refresh-ms-token edge function, since that exchange requires the Azure app's
    // client secret) and retry the request once.
    private async Task<HttpResponseMessage> SendWithRefreshAsync(HttpClient client, HttpMethod method, string relativeUrl, object? body = null)
    {
        var req = await BuildRequestAsync(method, relativeUrl, body);
        var resp = await client.SendAsync(req);
        if (resp.StatusCode != HttpStatusCode.Unauthorized) return resp;

        if (await RefreshAccessTokenAsync() is null) return resp;

        var retryReq = await BuildRequestAsync(method, relativeUrl, body);
        return await client.SendAsync(retryReq);
    }

    private async Task<string?> RefreshAccessTokenAsync()
    {
        var refreshToken = await js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenStorageKey);
        if (string.IsNullOrEmpty(refreshToken)) return null;
        try
        {
            // The refresh-ms-token function requires the caller's Supabase JWT (verify_jwt),
            // passed here as the Functions client's bearer token. CurrentSession can be
            // transiently empty (e.g. right after a reload, before GoTrue rehydrates from
            // storage), so reload once from local storage before giving up.
            var jwt = supabase.Auth.CurrentSession?.AccessToken;
            if (string.IsNullOrEmpty(jwt))
            {
                supabase.Auth.LoadSession();
                await supabase.Auth.RetrieveSessionAsync();
                jwt = supabase.Auth.CurrentSession?.AccessToken;
            }
            if (string.IsNullOrEmpty(jwt)) return null;

            var options = new Supabase.Functions.Client.InvokeFunctionOptions
            {
                Body = new Dictionary<string, object> { ["refresh_token"] = refreshToken }
            };
            var result = await supabase.Functions.Invoke<RefreshTokenResponse>("refresh-ms-token", jwt, options);
            if (string.IsNullOrEmpty(result?.AccessToken)) return null;

            await js.InvokeVoidAsync("localStorage.setItem", AccessTokenStorageKey, result.AccessToken);
            if (!string.IsNullOrEmpty(result.RefreshToken))
                await js.InvokeVoidAsync("localStorage.setItem", RefreshTokenStorageKey, result.RefreshToken);

            return result.AccessToken;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetOrCreateCalendarAsync(Workspace workspace)
    {
        if (string.IsNullOrEmpty(await GetAccessTokenAsync())) return null;
        if (workspace.Id is not Guid workspaceId) return null;
        var calendarName = $"TimeSight/{workspace.Name}";
        var storageKey = CalendarStorageKey(workspaceId);
        try
        {
            var stored = await js.InvokeAsync<string?>("localStorage.getItem", storageKey);
            if (!string.IsNullOrEmpty(stored)) return stored;

            var client = httpClientFactory.CreateClient("MicrosoftGraph");

            var listResp = await SendWithRefreshAsync(client, HttpMethod.Get, "me/calendars");
            if (listResp.IsSuccessStatusCode)
            {
                var listData = await listResp.Content.ReadFromJsonAsync<GraphListResponse<GraphCalendar>>();
                var existing = listData?.Value?.FirstOrDefault(c =>
                    string.Equals(c.Name, calendarName, StringComparison.OrdinalIgnoreCase));
                if (existing?.Id is not null)
                {
                    await js.InvokeVoidAsync("localStorage.setItem", storageKey, existing.Id);
                    return existing.Id;
                }
            }

            var createResp = await SendWithRefreshAsync(client, HttpMethod.Post, "me/calendars", new { name = calendarName });
            if (!createResp.IsSuccessStatusCode) return null;

            var created = await createResp.Content.ReadFromJsonAsync<GraphCalendar>();
            if (created?.Id is null) return null;

            await js.InvokeVoidAsync("localStorage.setItem", storageKey, created.Id);
            return created.Id;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> CreateEventAsync(string calendarId, Chore chore)
    {
        if (string.IsNullOrEmpty(await GetAccessTokenAsync())) return null;
        try
        {
            var client = httpClientFactory.CreateClient("MicrosoftGraph");
            var resp = await SendWithRefreshAsync(client, HttpMethod.Post, $"me/calendars/{calendarId}/events", BuildEventBody(chore));
            if (!resp.IsSuccessStatusCode) return null;
            var evt = await resp.Content.ReadFromJsonAsync<GraphEvent>();
            return evt?.Id;
        }
        catch { return null; }
    }

    public async Task UpdateEventAsync(string calendarId, string eventId, Chore chore)
    {
        if (string.IsNullOrEmpty(await GetAccessTokenAsync())) return;
        try
        {
            var client = httpClientFactory.CreateClient("MicrosoftGraph");
            await SendWithRefreshAsync(client, new HttpMethod("PATCH"), $"me/calendars/{calendarId}/events/{eventId}", BuildEventBody(chore));
        }
        catch { }
    }

    public async Task DeleteEventAsync(string calendarId, string eventId)
    {
        if (string.IsNullOrEmpty(await GetAccessTokenAsync())) return;
        try
        {
            var client = httpClientFactory.CreateClient("MicrosoftGraph");
            await SendWithRefreshAsync(client, HttpMethod.Delete, $"me/calendars/{calendarId}/events/{eventId}");
        }
        catch { }
    }

    public async Task<Dictionary<Guid, string>> LoadEventMapAsync(Guid workspaceId)
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", EventMapStorageKey(workspaceId));
            if (string.IsNullOrEmpty(json)) return [];
            return JsonSerializer.Deserialize<Dictionary<Guid, string>>(json) ?? [];
        }
        catch { return []; }
    }

    public async Task SaveEventMapAsync(Guid workspaceId, Dictionary<Guid, string> map)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", EventMapStorageKey(workspaceId),
                JsonSerializer.Serialize(map));
        }
        catch { }
    }

    private static object BuildEventBody(Chore chore)
    {
        var startMinutes = chore.StartTime ?? 9 * 60;
        var start = chore.StartDate.HasValue
            ? chore.StartDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(startMinutes)))
            : DateTime.UtcNow;

        DateTime end;
        if (chore.Deadline.HasValue && chore.StartDate.HasValue)
            end = chore.Deadline.Value.ToDateTime(new TimeOnly(23, 0));
        else
            end = start.AddHours(1);

        if (end <= start) end = start.AddHours(1);

        return new
        {
            subject = chore.Name,
            body = new { contentType = "text", content = chore.Description ?? string.Empty },
            start = new { dateTime = start.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" },
            end = new { dateTime = end.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" }
        };
    }

    private record GraphListResponse<T>([property: JsonPropertyName("value")] List<T>? Value);
    private record GraphCalendar([property: JsonPropertyName("id")] string? Id, [property: JsonPropertyName("name")] string? Name);
    private record GraphEvent([property: JsonPropertyName("id")] string? Id);

    // Deserialized via Newtonsoft.Json by Supabase.Functions's Invoke<T>, not System.Text.Json.
    private record RefreshTokenResponse(
        [property: Newtonsoft.Json.JsonProperty("access_token")] string? AccessToken,
        [property: Newtonsoft.Json.JsonProperty("refresh_token")] string? RefreshToken);
}
