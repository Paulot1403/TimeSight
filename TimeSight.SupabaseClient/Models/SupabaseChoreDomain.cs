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
    [PrimaryKey("chore_id", true)]
    public Guid ChoreId { get; set; }

    [PrimaryKey("domain_id", true)]
    public Guid DomainId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    public SupabaseChore? Chore { get; set; } = null!;
    public SupabaseDomain? Domain { get; set; } = null!;

    public bool ShouldSerializeChore() => false;
    public bool ShouldSerializeDomain() => false;
}
