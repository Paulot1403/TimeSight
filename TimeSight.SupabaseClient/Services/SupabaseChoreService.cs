using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using TimeSight.SupabaseClient.Models;
using static Supabase.Postgrest.Constants;
namespace TimeSight.SupabaseClient.Services;

public class SupabaseChoreService(Client supabase)
{
    public async Task<ICollection<SupabaseChore>> GetChoresAsync()
    {
        var response = await supabase.From<SupabaseChore>()
            .Select(@"
                *,
                SupabaseChoreDomains:chore_domain(
                    *
                )
            ")
            .Get();
        return response.Models;
    }
    public async Task<SupabaseChore> CreateChoreAsync(SupabaseChore supabaseChore)
    {
        var response = await supabase.From<SupabaseChore>().Insert(supabaseChore);
        return response.Model;
    }
    public async Task<SupabaseChore> UpdateChoreAsync(SupabaseChore supabaseChore)
    {
        var response = await supabase.From<SupabaseChore>().Update(supabaseChore);
        return response.Model;
    }
    public async Task DeleteChoreAsync(Guid choreId)
    {
        await supabase.From<SupabaseChore>()
            .Where(c => c.Id == choreId)
            .Delete();
    }

    public async Task AddChoreDomainAsync(SupabaseChoreDomain choreDomain)
    {
        await supabase.From<SupabaseChoreDomain>().Insert(choreDomain);
    }

    public async Task RemoveChoreDomainAsync(Guid choreId, Guid domainId)
    {
        await supabase.From<SupabaseChoreDomain>()
            .Where(cd => cd.ChoreId == choreId && cd.DomainId == domainId)
            .Delete();
    }
}
