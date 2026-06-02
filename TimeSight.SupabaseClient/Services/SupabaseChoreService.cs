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
    public async Task<SupabaseChore> CreateChore(SupabaseChore supabaseChore)
    {
        var response = await supabase.From<SupabaseChore>().Insert(supabaseChore);
        return response.Model;
    }
    public async Task<ICollection<SupabaseChore>> GetChores(Guid userId)
    {
        string userIdString = userId.ToString();
        var response = await supabase.From<SupabaseChore>()
            .Select(@"
                *,
                SupabaseChoreDomains:chore_domain(
                    *,
                    Domain:domains(*)
                )
            ")
            .Get();
        return response.Models;

        // var response = await supabase.From<SupabaseChore>()
        //     .Select(@"
        //         *,
        //         SupabaseChoreDomains:chore_domain!inner(
        //             *,
        //             Domain:domains!inner(*)
        //         )
        //     ")
        //     .Get();

        // var response = await supabase.From<Chore>()
        //     .Filter("user_id", Operator.Equals, userId)
        //     .Select(@"
        //         *,
        //         TestDomains:TestDomain!inner(
        //             *,
        //             Domain!inner(*)
        //         )
        //     ")
        //     .Filter("TestDomain.user_id", Operator.Equals, userId)
        //     .Get();
        // var stringR = response.Content;
        // return response.Models;
    }
}
