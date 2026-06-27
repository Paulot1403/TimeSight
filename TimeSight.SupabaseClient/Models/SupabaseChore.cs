using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;


[Table("chores")]
public class SupabaseChore : BaseModel
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }

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

    [Column("importance")]
    public int? Importance { get; set; }
    /// <summary>
    /// De 1 à 4 ?
    /// </summary>
    ///     
    [Column("duration")]
    public int? Duration { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("parent_chore_id")]
    public Guid? ParentChoreId { get; set; }

    [Column("done_at")]
    public DateTime? DoneAt { get; set; }

    /// <summary>
    /// Stocké en heures malgré le nom de colonne ("recurrence_interval_days" n'a pas pu être migré).
    /// </summary>
    [Column("recurrence_interval_days")]
    public int? RecurrenceIntervalHours { get; set; }

    [Column("recurrence_days_of_week")]
    public int? RecurrenceDaysOfWeek { get; set; }

    [Column("recurrence_reset_time")]
    public int? RecurrenceResetTime { get; set; }

    [Column("deadline")]
    public DateOnly? Deadline { get; set; }

    [Column("emergency_threshold_days")]
    public int? EmergencyThresholdDays { get; set; }

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("start_time")]
    public int? StartTime { get; set; }

    public List<SupabaseChoreDomain> SupabaseChoreDomains { get; set; } = [];
    public bool ShouldSerializeSupabaseChoreDomains() => false;

}
