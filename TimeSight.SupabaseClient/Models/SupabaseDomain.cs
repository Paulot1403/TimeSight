using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

[Table("domains")]
public class SupabaseDomain : BaseModel
{
    [Column("user_id")]
    public Guid UserId { get; set; }
    [Column("name")]
    public string Name { get; set; } = "";
    [Column("color")]
    public string? Color { get; set; }
    [Column("description")]
    public string? Description { get; set; }
    [PrimaryKey("id", false)]
    public Guid? Id { get; set; }
    public List<SupabaseChoreDomain> SupabaseChoreDomains { get; set; } = [];

    public bool ShouldSerializeSupabaseChoreDomains() => false;

}
