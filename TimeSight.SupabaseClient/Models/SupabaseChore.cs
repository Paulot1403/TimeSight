using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

public class SupabaseChore : BaseModel
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }

    /// <summary>
    /// Id from database
    /// </summary>
    ///
    [PrimaryKey]
    public int? Id { get; set; }
    public bool IsDone { get; set; } = false;

    /// <summary>
    /// De 1 à 4 ?
    /// </summary>
    public int? Significance { get; set; }
    /// <summary>
    /// De 1 à 4 ?
    /// </summary>
    public int? Duration { get; set; }

    public List<SupabaseChoreDomain> SupabaseChoreDomains { get; set; } = [];

    public DateTime? DoneDate { get; set; }

    // public DateTime? ScheduledStartDate { get; set; }
    // public DateTime? ScheduledEndDate { get; set; }

}
