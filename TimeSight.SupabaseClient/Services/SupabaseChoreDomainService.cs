using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using TimeSight.SupabaseClient.Models;
using static Supabase.Postgrest.Constants;
namespace TimeSight.SupabaseClient.Services;

public class SupabaseChoreDomainService(Client supabase)
{
    public async Task CreateChoreDomainAsync(SupabaseChoreDomain choreDomain)
    {
        await supabase.From<SupabaseChoreDomain>().Insert(choreDomain);
    }
    public async Task UpdateChoreDomainAsync(SupabaseChoreDomain choreDomain)
    {
        await supabase.From<SupabaseChoreDomain>().Update(choreDomain);
    }

    public async Task DeleteChoreDomainAsync(Guid choreId, Guid domainId)
    {
        await supabase.From<SupabaseChoreDomain>()
            .Where(cd => cd.ChoreId == choreId && cd.DomainId == domainId)
            .Delete();
    }
}
