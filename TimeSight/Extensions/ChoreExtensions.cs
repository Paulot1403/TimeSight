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
            Name = chore.Name,
            IsDone = chore.IsDone,
            Significance = chore.Significance,
            Duration = chore.Duration,
            DoneAt = chore.DoneAt,
            SupabaseChoreDomains = chore.ChoreDomains.Select(cd => cd.ToSupabaseChoreDomain()).ToList()
        };
    }
}
