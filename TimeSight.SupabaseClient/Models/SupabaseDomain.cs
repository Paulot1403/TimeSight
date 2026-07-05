using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

[Table("domains")]
public class SupabaseDomain : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid? Id { get; set; }
    [Column("user_id")]
    public Guid UserId { get; set; }
    [Column("workspace_id")]
    public Guid WorkspaceId { get; set; }
    [Column("name")]
    public string Name { get; set; } = "";
    [Column("color")]
    public string? Color { get; set; }
    [Column("description")]
    public string? Description { get; set; }

    [Column("done_score")]
    public string DoneScore { get; set; }

    public List<SupabaseChoreDomain> SupabaseChoreDomains { get; set; } = [];

    public bool ShouldSerializeSupabaseChoreDomains() => false;

}
