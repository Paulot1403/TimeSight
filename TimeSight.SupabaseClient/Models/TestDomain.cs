using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

public class TestDomain : BaseModel
{
    public int TestId { get; set; }
    public int DomainId { get; set; }

    public Test? Test { get; set; }
    public Domain? Domain { get; set; }
}
