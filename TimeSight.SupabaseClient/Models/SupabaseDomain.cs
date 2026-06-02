using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

public class SupabaseDomain : BaseModel
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    [PrimaryKey]
    public int? Id { get; set; }
    public List<SupabaseChoreDomain> SupabaseChoreDomains { get; set; } = [];
}
