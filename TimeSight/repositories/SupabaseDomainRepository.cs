using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Extensions;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;
using TimeSight.SupabaseClient.Services;

namespace TimeSight.repositories;

public class SupabaseDomainRepository(SupabaseDomainService supabaseDomainService) : IDomainRepository
{
    public async Task<Domain> GetDomainAsync(Guid domainId)
    {
        SupabaseDomain supabaseDomain = await supabaseDomainService.GetDomainAsync(domainId);
        return supabaseDomain.ToDomain();
    }

    public async Task<List<Domain>> GetDomainsAsync()
    {
        var supabaseDomains = await supabaseDomainService.GetDomainsAsync();
        return supabaseDomains.Select(sd => sd.ToDomain()).ToList();
    }
    public async Task<Domain> CreateDomainAsync(Domain domain)
    {
        SupabaseDomain supabaseDomain = domain.ToSupabaseDomain();
        supabaseDomain = await supabaseDomainService.CreateDomainAsync(supabaseDomain);
        return supabaseDomain.ToDomain();
    }
    public async Task<Domain> UpdateDomainAsync(Domain domain)
    {
        SupabaseDomain supabaseDomain = domain.ToSupabaseDomain();
        supabaseDomain = await supabaseDomainService.UpdateDomainAsync(supabaseDomain);
        return supabaseDomain.ToDomain();
    }
    public async Task DeleteDomainAsync(Guid domainId)
    {
        await supabaseDomainService.DeleteDomainAsync(domainId);
    }

}
