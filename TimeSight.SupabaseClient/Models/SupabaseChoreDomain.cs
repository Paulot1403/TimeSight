using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Postgrest.Models;

namespace TimeSight.SupabaseClient.Models;

public class SupabaseChoreDomain : BaseModel
{
    public required Guid UserId { get; set; }
    public required int ChoreId { get; set; }
    public required int DomainId { get; set; }
    public int Intensity { get; set; } = 2;

    public SupabaseChore? Chore { get; set; } = null!;
    public SupabaseDomain? Domain { get; set; } = null!;
}
