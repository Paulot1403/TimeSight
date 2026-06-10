using TimeSight.Models;

namespace TimeSight.repositories;

public interface IWorkspaceRepository
{
    Task<List<Workspace>> GetWorkspacesAsync();
    Task<Workspace> CreateWorkspaceAsync(Workspace workspace);
    Task<Workspace> UpdateWorkspaceAsync(Workspace workspace);
    Task DeleteWorkspaceAsync(Guid workspaceId);
}
