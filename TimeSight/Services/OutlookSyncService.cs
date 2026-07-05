using TimeSight.Models;

namespace TimeSight.Services;

public class OutlookSyncService(MicrosoftGraphService graphService, WorkspaceState workspaceState)
{
    private readonly Dictionary<Guid, string> _calendarIdCache = new();

    private async Task<(string? calendarId, Guid workspaceId)> EnsureCalendarAsync()
    {
        var workspace = workspaceState.CurrentWorkspace;
        if (workspace?.Id is not Guid wsId) return (null, default);

        if (_calendarIdCache.TryGetValue(wsId, out var cached))
            return (cached, wsId);

        var calId = await graphService.GetOrCreateCalendarAsync(workspace);
        if (calId is not null)
            _calendarIdCache[wsId] = calId;
        return (calId, wsId);
    }

    public async Task SyncChoreCreatedAsync(Chore chore)
    {
        if (!chore.ShouldBeOnCalendar) return;
        var (calId, wsId) = await EnsureCalendarAsync();
        if (calId is null) return;

        var eventId = await graphService.CreateEventAsync(calId, chore);
        if (eventId is null) return;

        var map = await graphService.LoadEventMapAsync(wsId);
        map[chore.Id] = eventId;
        await graphService.SaveEventMapAsync(wsId, map);
    }

    public async Task SyncChoreUpdatedAsync(Chore chore)
    {
        var (calId, wsId) = await EnsureCalendarAsync();
        if (calId is null) return;

        var map = await graphService.LoadEventMapAsync(wsId);

        if (map.TryGetValue(chore.Id, out var existingEventId))
        {
            if (chore.ShouldBeOnCalendar)
            {
                await graphService.UpdateEventAsync(calId, existingEventId, chore);
            }
            else
            {
                await graphService.DeleteEventAsync(calId, existingEventId);
                map.Remove(chore.Id);
                await graphService.SaveEventMapAsync(wsId, map);
            }
        }
        else if (chore.ShouldBeOnCalendar)
        {
            var eventId = await graphService.CreateEventAsync(calId, chore);
            if (eventId is not null)
            {
                map[chore.Id] = eventId;
                await graphService.SaveEventMapAsync(wsId, map);
            }
        }
    }

    public async Task SyncChoreDeletedAsync(Guid choreId)
    {
        var (calId, wsId) = await EnsureCalendarAsync();
        if (calId is null) return;

        var map = await graphService.LoadEventMapAsync(wsId);
        if (map.TryGetValue(choreId, out var eventId))
        {
            await graphService.DeleteEventAsync(calId, eventId);
            map.Remove(choreId);
            await graphService.SaveEventMapAsync(wsId, map);
        }
    }
}
