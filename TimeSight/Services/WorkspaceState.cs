using TimeSight.Models;

namespace TimeSight.Services;

public class WorkspaceState
{
    public Workspace? CurrentWorkspace { get; private set; }

    public event Action? OnChanged;

    public void SetWorkspace(Workspace workspace)
    {
        CurrentWorkspace = workspace;
        OnChanged?.Invoke();
    }
}
