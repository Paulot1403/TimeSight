using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

[Table("domain")]
public class SupabaseDomain : BaseModel
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    [PrimaryKey]
    public Guid? Id { get; set; }
    public List<SupabaseChoreDomain> SupabaseChoreDomains { get; set; } = [];
}
