namespace Realtime_ToDo_Web_API.Models;

public class WorkspaceInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TaskCount { get; set; }

    public WorkspaceInfo(Workspace workspace) 
    {
        Id = workspace.Id;
        Name = workspace.Name;
        TaskCount = workspace.Tasks?.Count ?? 0;
    }
}
