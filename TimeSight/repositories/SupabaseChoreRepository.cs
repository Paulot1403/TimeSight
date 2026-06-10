using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase.Gotrue;
using TimeSight.Extensions;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;
using TimeSight.SupabaseClient.Services;

namespace TimeSight.repositories;

public class SupabaseChoreRepository(SupabaseChoreService supabaseChoreService) : IChoreRepository
{
    private static int _taskIncrement = 1;
    public async Task<List<Chore>> GetChoresAsync(Guid workspaceId)
    {
        ICollection<SupabaseChore> supabaseChores = await supabaseChoreService.GetChoresAsync(workspaceId);
        return [.. supabaseChores.Select(sc => sc.ToChore())];
    }

    public async Task<Chore> CreateChoreAsync(Guid userId, Guid workspaceId, string name = "")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Task " + _taskIncrement++;
        }

        SupabaseChore supabaseChore = new() { UserId = userId, WorkspaceId = workspaceId, Name = name };

        SupabaseChore newSupabaseChore = await supabaseChoreService.CreateChoreAsync(supabaseChore);

        return newSupabaseChore.ToChore();
    }
    public async Task<Chore> UpdateChoreAsync(Chore chore)
    {
        SupabaseChore supabaseChore = chore.ToSupabaseChore();

        SupabaseChore newSupabaseChore = await supabaseChoreService.UpdateChoreAsync(supabaseChore);

        return newSupabaseChore.ToChore();
    }
    public async Task DeleteChoreAsync(Guid choreId)
    {
        await supabaseChoreService.DeleteChoreAsync(choreId);
    }

}
