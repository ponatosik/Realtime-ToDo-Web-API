namespace Realtime_ToDo_Web_API.Models;

public class WorkspaceInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "New workspace";
    public int TaskCount { get; set; }

    public WorkspaceInfo(Workspace workspace, int? taskCount)
    {
        Id = workspace.Id;
        Name = workspace.Name;
        TaskCount = taskCount ?? 0;
    }
    public WorkspaceInfo(Workspace workspace) : this(workspace, workspace.Tasks?.Count) { }
    public WorkspaceInfo() { }
}
