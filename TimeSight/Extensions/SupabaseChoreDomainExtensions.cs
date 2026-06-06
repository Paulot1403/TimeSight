using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.Extensions;

public static class SupabaseChoreDomainExtensions
{
    public static ChoreDomain ToChoreDomain(this SupabaseChoreDomain supabaseChoreDomain)
    {
        return new ChoreDomain()
        {
            UserId = supabaseChoreDomain.UserId,
            ChoreId = supabaseChoreDomain.ChoreId,
            DomainId = supabaseChoreDomain.DomainId,
            LinkIntensity = supabaseChoreDomain.LinkIntensity,
        };
    }
}
