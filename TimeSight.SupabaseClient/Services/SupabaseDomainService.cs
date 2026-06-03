using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.SupabaseClient.Services;

public class SupabaseDomainService(Client supabase)
{
    public async Task<SupabaseDomain> CreateDomainAsync(SupabaseDomain supabaseDomain)
    {
        var response = await supabase.From<SupabaseDomain>().Insert(supabaseDomain);
        ArgumentNullException.ThrowIfNull(response.Model);
        return response.Model;
    }
    public async Task<ICollection<SupabaseDomain>> GetDomainsAsync()
    {
        var response = await supabase.From<SupabaseDomain>()
            .Select(@"
                *
            ")
            .Get();
        return response.Models;
    }

    public async Task<SupabaseDomain> GetDomainAsync(Guid domainId)
    {
        var response = await supabase.From<SupabaseDomain>()
            .Select(@"
                *
            ")
            .Where(d => d.Id == domainId)
            .Get();
        ArgumentNullException.ThrowIfNull(response.Model);
        return response.Model;
    }
}
