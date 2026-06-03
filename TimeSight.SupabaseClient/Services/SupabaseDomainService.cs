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
}
