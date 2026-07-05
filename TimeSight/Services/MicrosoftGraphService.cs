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
            var resp = await SendWithRefreshAsync(client, HttpMethod.Post, $"me/calendars/{calendarId}/events", await BuildEventBodyAsync(chore));
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
            await SendWithRefreshAsync(client, new HttpMethod("PATCH"), $"me/calendars/{calendarId}/events/{eventId}", await BuildEventBodyAsync(chore));
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

    private async Task<object> BuildEventBodyAsync(Chore chore)
    {
        var startMinutes = chore.StartTime ?? chore.RecurrenceResetTime ?? 9 * 60;

        DateTime start;
        if (chore.StartDate.HasValue)
            start = chore.StartDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(startMinutes)));
        else if (chore.RecurrenceResetTime.HasValue)
            start = ComputeNextOccurrence(DateTime.Now, chore.RecurrenceResetTime.Value, chore.RecurrenceDaysOfWeek);
        else
            start = DateTime.Now;

        var durationMinutes = chore.Duration.HasValue ? chore.Duration.Value * 15 : 60;
        DateTime end = start.AddMinutes(durationMinutes);

        // Times above are the user's local wall-clock time (e.g. "18:00"), so the event
        // must be tagged with the browser's actual IANA time zone rather than "UTC" -
        // otherwise Outlook re-interprets "18:00" as UTC and shifts it by the local offset.
        var timeZone = await js.InvokeAsync<string>("blazorHelpers.getTimeZone");

        var subject = chore.Name;
        var body = new { contentType = "text", content = chore.Description ?? string.Empty };
        var startDto = new { dateTime = start.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone };
        var endDto = new { dateTime = end.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone };
        var recurrence = BuildRecurrence(chore, start);

        if (recurrence is null)
            return new { subject, body, start = startDto, end = endDto };

        return new { subject, body, start = startDto, end = endDto, recurrence };
    }

    private static object? BuildRecurrence(Chore chore, DateTime eventStart)
    {
        var startDate = DateOnly.FromDateTime(eventStart).ToString("yyyy-MM-dd");

        if (chore.RecurrenceResetTime.HasValue)
        {
            if (chore.RecurrenceDaysOfWeek is null)
                return new
                {
                    pattern = new { type = "daily", interval = 1 },
                    range = new { type = "noEnd", startDate }
                };

            var days = DaysOfWeekMaskToNames(chore.RecurrenceDaysOfWeek.Value);
            if (days.Length == 0) return null;
            return new
            {
                pattern = new { type = "weekly", interval = 1, daysOfWeek = days },
                range = new { type = "noEnd", startDate }
            };
        }

        if (chore.RecurrenceIntervalHours.HasValue)
        {
            var dayName = GraphDayName(eventStart.DayOfWeek);
            var dayOfMonth = eventStart.Day;
            switch (chore.RecurrenceIntervalHours.Value)
            {
                case 24:
                    return new { pattern = new { type = "daily", interval = 1 }, range = new { type = "noEnd", startDate } };
                case 72:
                    return new { pattern = new { type = "daily", interval = 3 }, range = new { type = "noEnd", startDate } };
                case 168:
                    return new { pattern = new { type = "weekly", interval = 1, daysOfWeek = new[] { dayName } }, range = new { type = "noEnd", startDate } };
                case 336:
                    return new { pattern = new { type = "weekly", interval = 2, daysOfWeek = new[] { dayName } }, range = new { type = "noEnd", startDate } };
                case 720:
                    return new { pattern = new { type = "absoluteMonthly", interval = 1, dayOfMonth }, range = new { type = "noEnd", startDate } };
                case 1440:
                    return new { pattern = new { type = "absoluteMonthly", interval = 2, dayOfMonth }, range = new { type = "noEnd", startDate } };
            }
        }

        return null;
    }

    private static string[] DaysOfWeekMaskToNames(int mask)
    {
        var days = new List<string>();
        if ((mask & 1) != 0) days.Add("monday");
        if ((mask & 2) != 0) days.Add("tuesday");
        if ((mask & 4) != 0) days.Add("wednesday");
        if ((mask & 8) != 0) days.Add("thursday");
        if ((mask & 16) != 0) days.Add("friday");
        if ((mask & 32) != 0) days.Add("saturday");
        if ((mask & 64) != 0) days.Add("sunday");
        return [.. days];
    }

    private static string GraphDayName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "monday",
        DayOfWeek.Tuesday => "tuesday",
        DayOfWeek.Wednesday => "wednesday",
        DayOfWeek.Thursday => "thursday",
        DayOfWeek.Friday => "friday",
        DayOfWeek.Saturday => "saturday",
        DayOfWeek.Sunday => "sunday",
        _ => "monday"
    };

    private static DateTime ComputeNextOccurrence(DateTime from, int resetTimeMinutes, int? daysOfWeekMask)
    {
        var time = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(resetTimeMinutes));
        for (var day = DateOnly.FromDateTime(from); ; day = day.AddDays(1))
        {
            if (daysOfWeekMask is null || MatchesDayMask(day.DayOfWeek, daysOfWeekMask.Value))
            {
                var candidate = day.ToDateTime(time);
                if (candidate > from) return candidate;
            }
        }
    }

    private static bool MatchesDayMask(DayOfWeek dayOfWeek, int mask)
    {
        var bit = dayOfWeek switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 4,
            DayOfWeek.Thursday => 8,
            DayOfWeek.Friday => 16,
            DayOfWeek.Saturday => 32,
            DayOfWeek.Sunday => 64,
            _ => 0
        };
        return (mask & bit) != 0;
    }

    private record GraphListResponse<T>([property: JsonPropertyName("value")] List<T>? Value);
    private record GraphCalendar([property: JsonPropertyName("id")] string? Id, [property: JsonPropertyName("name")] string? Name);
    private record GraphEvent([property: JsonPropertyName("id")] string? Id);

    // Deserialized via Newtonsoft.Json by Supabase.Functions's Invoke<T>, not System.Text.Json.
    private record RefreshTokenResponse(
        [property: Newtonsoft.Json.JsonProperty("access_token")] string? AccessToken,
        [property: Newtonsoft.Json.JsonProperty("refresh_token")] string? RefreshToken);
}
