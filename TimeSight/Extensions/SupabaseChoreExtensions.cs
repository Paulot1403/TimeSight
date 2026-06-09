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
            Name = supabaseChore.Name,
            IsDone = supabaseChore.IsDone,
            Duration = supabaseChore.Duration ?? 2,
            Description = supabaseChore.Description,
            ParentChoreId = supabaseChore.ParentChoreId,
            DoneAt = supabaseChore.DoneAt,
            ChoreDomains = supabaseChore.SupabaseChoreDomains.Select(cd => cd.ToChoreDomain()).ToList()
        };
    }
}
