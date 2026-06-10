using TimeSight.Models;
using TimeSight.SupabaseClient.Models;

namespace TimeSight.Extensions;

public static class SupabaseWorkspaceExtensions
{
    public static Workspace ToWorkspace(this SupabaseWorkspace sw)
    {
        return new Workspace
        {
            Id = sw.Id,
            UserId = sw.UserId,
            Name = sw.Name,
            Color = sw.Color,
            Description = sw.Description,
            IsDefault = sw.IsDefault ?? false
        };
    }
}
