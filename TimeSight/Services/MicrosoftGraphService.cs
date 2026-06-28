using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using TimeSight.Models;

namespace TimeSight.Services;

public class MicrosoftGraphService(Supabase.Client supabase, IHttpClientFactory httpClientFactory, IJSRuntime js)
{
    private const string CalendarStorageKey = "timesight.outlook-calendar-id";
    private const string EventMapStorageKey = "timesight.outlook-event-map";

    private string? GetAccessToken() => supabase.Auth.CurrentSession?.ProviderToken;

    private HttpRequestMessage BuildRequest(HttpMethod method, string relativeUrl, object? body = null)
    {
        var req = new HttpRequestMessage(method, relativeUrl);
        var token = GetAccessToken();
        if (token is not null)
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            req.Content = JsonContent.Create(body);
        return req;
    }

    public async Task<string?> GetOrCreateCalendarAsync()
    {
        if (string.IsNullOrEmpty(GetAccessToken())) return null;
        try
        {
            var stored = await js.InvokeAsync<string?>("localStorage.getItem", CalendarStorageKey);
            if (!string.IsNullOrEmpty(stored)) return stored;

            var client = httpClientFactory.CreateClient("MicrosoftGraph");

            var listReq = BuildRequest(HttpMethod.Get, "me/calendars");
            var listResp = await client.SendAsync(listReq);
            if (listResp.IsSuccessStatusCode)
            {
                var listData = await listResp.Content.ReadFromJsonAsync<GraphListResponse<GraphCalendar>>();
                var existing = listData?.Value?.FirstOrDefault(c =>
                    string.Equals(c.Name, "TimeSight", StringComparison.OrdinalIgnoreCase));
                if (existing?.Id is not null)
                {
                    await js.InvokeVoidAsync("localStorage.setItem", CalendarStorageKey, existing.Id);
                    return existing.Id;
                }
            }

            var createReq = BuildRequest(HttpMethod.Post, "me/calendars", new { name = "TimeSight" });
            var createResp = await client.SendAsync(createReq);
            if (!createResp.IsSuccessStatusCode) return null;

            var created = await createResp.Content.ReadFromJsonAsync<GraphCalendar>();
            if (created?.Id is null) return null;

            await js.InvokeVoidAsync("localStorage.setItem", CalendarStorageKey, created.Id);
            return created.Id;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> CreateEventAsync(string calendarId, Chore chore)
    {
        if (string.IsNullOrEmpty(GetAccessToken())) return null;
        try
        {
            var client = httpClientFactory.CreateClient("MicrosoftGraph");
            var req = BuildRequest(HttpMethod.Post, $"me/calendars/{calendarId}/events", BuildEventBody(chore));
            var resp = await client.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;
            var evt = await resp.Content.ReadFromJsonAsync<GraphEvent>();
            return evt?.Id;
        }
        catch { return null; }
    }

    public async Task UpdateEventAsync(string calendarId, string eventId, Chore chore)
    {
        if (string.IsNullOrEmpty(GetAccessToken())) return;
        try
        {
            var client = httpClientFactory.CreateClient("MicrosoftGraph");
            var req = BuildRequest(new HttpMethod("PATCH"), $"me/calendars/{calendarId}/events/{eventId}", BuildEventBody(chore));
            await client.SendAsync(req);
        }
        catch { }
    }

    public async Task DeleteEventAsync(string calendarId, string eventId)
    {
        if (string.IsNullOrEmpty(GetAccessToken())) return;
        try
        {
            var client = httpClientFactory.CreateClient("MicrosoftGraph");
            var req = BuildRequest(HttpMethod.Delete, $"me/calendars/{calendarId}/events/{eventId}");
            await client.SendAsync(req);
        }
        catch { }
    }

    public async Task<Dictionary<Guid, string>> LoadEventMapAsync()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", EventMapStorageKey);
            if (string.IsNullOrEmpty(json)) return [];
            return JsonSerializer.Deserialize<Dictionary<Guid, string>>(json) ?? [];
        }
        catch { return []; }
    }

    public async Task SaveEventMapAsync(Dictionary<Guid, string> map)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", EventMapStorageKey,
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
}
