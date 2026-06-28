using TimeSight.Models;

namespace TimeSight.Services;

public class OutlookSyncService(MicrosoftGraphService graphService)
{
    private string? _calendarId;

    private async Task<string?> EnsureCalendarAsync()
    {
        _calendarId ??= await graphService.GetOrCreateCalendarAsync();
        return _calendarId;
    }

    public async Task SyncChoreCreatedAsync(Chore chore)
    {
        if (!HasCalendarDate(chore)) return;
        var calId = await EnsureCalendarAsync();
        if (calId is null) return;

        var eventId = await graphService.CreateEventAsync(calId, chore);
        if (eventId is null) return;

        var map = await graphService.LoadEventMapAsync();
        map[chore.Id] = eventId;
        await graphService.SaveEventMapAsync(map);
    }

    public async Task SyncChoreUpdatedAsync(Chore chore)
    {
        var calId = await EnsureCalendarAsync();
        if (calId is null) return;

        var map = await graphService.LoadEventMapAsync();

        if (map.TryGetValue(chore.Id, out var existingEventId))
        {
            if (HasCalendarDate(chore))
            {
                await graphService.UpdateEventAsync(calId, existingEventId, chore);
            }
            else
            {
                await graphService.DeleteEventAsync(calId, existingEventId);
                map.Remove(chore.Id);
                await graphService.SaveEventMapAsync(map);
            }
        }
        else if (HasCalendarDate(chore))
        {
            var eventId = await graphService.CreateEventAsync(calId, chore);
            if (eventId is not null)
            {
                map[chore.Id] = eventId;
                await graphService.SaveEventMapAsync(map);
            }
        }
    }

    public async Task SyncChoreDeletedAsync(Guid choreId)
    {
        var calId = await EnsureCalendarAsync();
        if (calId is null) return;

        var map = await graphService.LoadEventMapAsync();
        if (map.TryGetValue(choreId, out var eventId))
        {
            await graphService.DeleteEventAsync(calId, eventId);
            map.Remove(choreId);
            await graphService.SaveEventMapAsync(map);
        }
    }

    private static bool HasCalendarDate(Chore chore) =>
        chore.StartDate.HasValue || chore.Deadline.HasValue;
}
