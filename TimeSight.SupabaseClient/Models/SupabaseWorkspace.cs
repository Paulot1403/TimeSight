using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

[Table("workspaces")]
public class SupabaseWorkspace : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid? Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("color")]
    public string? Color { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_default")]
    public bool? IsDefault { get; set; }
}
