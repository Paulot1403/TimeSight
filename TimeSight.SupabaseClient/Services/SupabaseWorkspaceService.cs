using Supabase;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.SupabaseClient.Services;

public class SupabaseWorkspaceService(Client supabase)
{
    public async Task<ICollection<SupabaseWorkspace>> GetWorkspacesAsync()
    {
        var response = await supabase.From<SupabaseWorkspace>().Select("*").Get();
        return response.Models;
    }

    public async Task<SupabaseWorkspace> CreateWorkspaceAsync(SupabaseWorkspace workspace)
    {
        var response = await supabase.From<SupabaseWorkspace>().Insert(workspace);
        ArgumentNullException.ThrowIfNull(response.Model);
        return response.Model;
    }

    public async Task<SupabaseWorkspace> UpdateWorkspaceAsync(SupabaseWorkspace workspace)
    {
        var response = await supabase.From<SupabaseWorkspace>().Update(workspace);
        ArgumentNullException.ThrowIfNull(response.Model);
        return response.Model;
    }

    public async Task DeleteWorkspaceAsync(Guid workspaceId)
    {
        await supabase.From<SupabaseWorkspace>()
            .Where(w => w.Id == workspaceId)
            .Delete();
    }
}
