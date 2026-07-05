using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.Extensions;

public static class SupabaseChoreExtensions
{
    public static Chore ToChore(this SupabaseChore supabaseChore)
    {
        return new Chore
        {
            Id = supabaseChore.Id!.Value,
            UserId = supabaseChore.UserId,
            WorkspaceId = supabaseChore.WorkspaceId,
            Name = supabaseChore.Name,
            IsDone = supabaseChore.IsDone,
            Duration = supabaseChore.Duration,
            Description = supabaseChore.Description,
            ParentChoreId = supabaseChore.ParentChoreId,
            DoneAt = supabaseChore.DoneAt,
            RecurrenceIntervalHours = supabaseChore.RecurrenceIntervalHours,
            RecurrenceResetTime = supabaseChore.RecurrenceResetTime,
            RecurrenceDaysOfWeek = supabaseChore.RecurrenceDaysOfWeek,
            StartDate = supabaseChore.StartDate,
            StartTime = supabaseChore.StartTime,
            Importance = supabaseChore.Importance ?? Chore.DEFAULT_IMPORTANCE,
            ChoreDomains = supabaseChore.SupabaseChoreDomains.Select(cd => cd.ToChoreDomain()).ToList()
        };
    }
}
