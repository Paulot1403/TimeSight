using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;
using TimeSight.SupabaseClient.Services;

namespace TimeSight.repositories;

public class SupabaseChoreRepository(SupabaseChoreService supabaseChoreService) : IChoreRepository
{
    public async Task<ICollection<Chore>> GetChores(Guid userId)
    {
        ICollection<SupabaseChore> supabaseChores = await supabaseChoreService.GetChores(userId);
        return [.. supabaseChores.Select(sc =>
        {
            return new Chore() { UserId = sc.UserId, Name = sc.Name };
        })];

    }
}
