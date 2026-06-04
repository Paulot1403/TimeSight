using Microsoft.AspNetCore.Components;

namespace TimeSight.Extensions;

public static class NavigationManagerExtensions
{
    public static void NavigateToRelative(this NavigationManager nav, string relativePath, bool forceLoad = false)
    {
        nav.NavigateTo(nav.BaseUri.TrimEnd('/') + "/" + relativePath.TrimStart('/'), forceLoad);
    }
}
