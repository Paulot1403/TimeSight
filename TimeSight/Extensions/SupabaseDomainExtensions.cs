using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.Extensions;

public static class SupabaseDomainExtensions
{
    public static Domain ToDomain(this SupabaseDomain supabaseDomain)
    {
        return new()
        {
            Id = supabaseDomain.Id,
            UserId = supabaseDomain.UserId,
            WorkspaceId = supabaseDomain.WorkspaceId,
            Name = supabaseDomain.Name,
            Color = supabaseDomain.Color,
            Description = supabaseDomain.Description,
            Importance = supabaseDomain.Importance,
            DoneScore = supabaseDomain.DoneScore == null ? 1 : int.Parse(supabaseDomain.DoneScore)
        };
    }
}
