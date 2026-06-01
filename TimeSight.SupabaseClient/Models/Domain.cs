using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

public class Domain : BaseModel
{
    [PrimaryKey]
    public int Id { get; set; }

    public string Name { get; set; }
}
