using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.Extensions;

public static class ChoreDomainExtensions
{
    public static SupabaseChoreDomain ToSupabaseChoreDomain(this ChoreDomain choreDomain)
    {
        return new()
        {
            UserId = choreDomain.UserId,
            ChoreId = choreDomain.ChoreId,
            DomainId = choreDomain.DomainId
        };
    }
}
