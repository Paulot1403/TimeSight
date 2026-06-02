using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSight.Extensions;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;
using TimeSight.SupabaseClient.Services;

namespace TimeSight.repositories;

public class SupabaseChoreRepository(SupabaseChoreService supabaseChoreService) : IChoreRepository
{
    public async Task<ICollection<Chore>> GetChoresAsync(Guid userId)
    {
        ICollection<SupabaseChore> supabaseChores = await supabaseChoreService.GetChores(userId);
        return [.. supabaseChores.Select(sc =>
        {
            return new Chore() { UserId = sc.UserId, Name = sc.Name };
        })];

    }

    public async Task<Chore> CreateChoreAsync(Chore chore)
    {
        SupabaseChore supabaseChore = chore.ToSupabaseChore();

        SupabaseChore newSupabaseChore = await supabaseChoreService.CreateChore(supabaseChore);

        return newSupabaseChore.ToChore();
    }


}
