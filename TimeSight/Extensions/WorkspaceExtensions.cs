using TimeSight.Models;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.Extensions;

public static class WorkspaceExtensions
{
    public static SupabaseWorkspace ToSupabaseWorkspace(this Workspace workspace)
    {
        return new SupabaseWorkspace
        {
            Id = workspace.Id,
            UserId = workspace.UserId,
            Name = workspace.Name,
            Color = workspace.Color,
            Description = workspace.Description
        };
    }
}
