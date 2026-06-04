using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

[Table("chore_domain")]
public class SupabaseChoreDomain : BaseModel
{
    [Column("user_id")]
    public Guid UserId { get; set; }
    [Column("chore_id")]
    public Guid ChoreId { get; set; }
    [Column("domain_id")]
    public Guid DomainId { get; set; }
    [Column("link_intensity")]
    public int LinkIntensity { get; set; } = 2;

    public SupabaseChore? Chore { get; set; } = null!;
    public SupabaseDomain? Domain { get; set; } = null!;

    public bool ShouldSerializeChore() => false;
    public bool ShouldSerializeDomain() => false;
}
