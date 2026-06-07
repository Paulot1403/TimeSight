using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;


[Table("chores")]
public class SupabaseChore : BaseModel
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Id from database
    /// </summary>
    /// 
    [PrimaryKey("id", false)]
    public Guid? Id { get; set; }

    [Column("is_done")]
    public bool IsDone { get; set; } = false;

    /// <summary>
    /// De 1 à 4 ?
    /// </summary>
    ///     
    [Column("duration")]
    public int? Duration { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("done_at")]
    public DateTime? DoneAt { get; set; }

    public List<SupabaseChoreDomain> SupabaseChoreDomains { get; set; } = [];
    public bool ShouldSerializeSupabaseChoreDomains() => false;


    // public DateTime? ScheduledStartDate { get; set; }
    // public DateTime? ScheduledEndDate { get; set; }

}
