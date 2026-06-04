using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Extensions;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;
using TimeSight.SupabaseClient.Services;

namespace TimeSight.repositories;

public class SupabaseChoreDomainRepository(SupabaseChoreDomainService supabaseChoreDomainService)
    : IChoreDomainRepository
{

    public async Task CreateChoreDomainAsync(ChoreDomain choreDomain)
    {
        await supabaseChoreDomainService.CreateChoreDomainAsync(choreDomain.ToSupabaseChoreDomain());
    }

    public async Task UpdateChoreDomainAsync(ChoreDomain choreDomain)
    {
        await supabaseChoreDomainService.UpdateChoreDomainAsync(choreDomain.ToSupabaseChoreDomain());
    }
    public async Task DeleteChoreDomainAsync(Guid choreId, Guid domainId)
    {
        await supabaseChoreDomainService.DeleteChoreDomainAsync(choreId, domainId);
    }
}
