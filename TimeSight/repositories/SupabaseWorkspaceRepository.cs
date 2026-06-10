using TimeSight.Extensions;
using TimeSight.Models;
using TimeSight.SupabaseClient.Models;
using TimeSight.SupabaseClient.Services;

namespace TimeSight.repositories;

public class SupabaseWorkspaceRepository(SupabaseWorkspaceService supabaseWorkspaceService) : IWorkspaceRepository
{
    public async Task<List<Workspace>> GetWorkspacesAsync()
    {
        var result = await supabaseWorkspaceService.GetWorkspacesAsync();
        return result.Select(sw => sw.ToWorkspace()).ToList();
    }

    public async Task<Workspace> CreateWorkspaceAsync(Workspace workspace)
    {
        var sw = workspace.ToSupabaseWorkspace();
        sw = await supabaseWorkspaceService.CreateWorkspaceAsync(sw);
        return sw.ToWorkspace();
    }

    public async Task<Workspace> UpdateWorkspaceAsync(Workspace workspace)
    {
        var sw = workspace.ToSupabaseWorkspace();
        sw = await supabaseWorkspaceService.UpdateWorkspaceAsync(sw);
        return sw.ToWorkspace();
    }

    public async Task DeleteWorkspaceAsync(Guid workspaceId)
    {
        await supabaseWorkspaceService.DeleteWorkspaceAsync(workspaceId);
    }
}
