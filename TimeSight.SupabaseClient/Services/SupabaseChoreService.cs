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
    public async Task<ICollection<Test>> GetChores()
    {
        string userId = "84fd300b-ed96-4f89-b757-f2dfc3946757";
        var response = await supabase.From<Test>()
            .Filter("user_id", Operator.Equals, userId)
            .Select(@"
                *,
                TestDomains:TestDomain!inner(
                    *,
                    Domain!inner(*)
                )
            ")
            .Filter("TestDomain.user_id", Operator.Equals, userId)
            .Get();
        var stringR = response.Content;
        return response.Models;
    }
}
