using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.Extensions;

public static class ChoreExtensions
{
    public static SupabaseChore ToSupabaseChore(this Chore chore)
    {
        return new SupabaseChore
        {
            Id = chore.Id,
            UserId = chore.UserId,
            WorkspaceId = chore.WorkspaceId,
            Name = chore.Name,
            IsDone = chore.IsDone,
            Duration = chore.Duration,
            Description = chore.Description,
            ParentChoreId = chore.ParentChoreId,
            DoneAt = chore.DoneAt,
            RecurrenceIntervalHours = chore.RecurrenceIntervalHours,
            RecurrenceResetTime = chore.RecurrenceResetTime,
            RecurrenceDaysOfWeek = chore.RecurrenceDaysOfWeek,
            StartDate = chore.StartDate,
            StartTime = chore.StartTime,
            Importance = chore.Importance,
            SupabaseChoreDomains = chore.ChoreDomains.Select(cd => cd.ToSupabaseChoreDomain()).ToList()
        };
    }
}
