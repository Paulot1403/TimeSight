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
            Name = supabaseDomain.Name,
            Color = supabaseDomain.Color
        };
    }
}
