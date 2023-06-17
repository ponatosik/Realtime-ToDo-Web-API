namespace Realtime_ToDo_Web_API.Models;

/// <summary>
/// Represents information about a workspace.
/// </summary>
/// <remarks>
/// This model is used to represent workspace to user.
/// To see how workspace is stored in database see <see cref="Workspace"/>
/// </remarks>
public class WorkspaceInfo
{
    /// <summary>
    /// The unique identifier of the workspace.
    /// </summary>
    /// <remarks>
    /// Should not be changed by user
    /// </remarks>
    public int Id { get; set; }

    /// <summary>
    /// The name of the workspace.
    /// </summary>
    public string Name { get; set; } = "New workspace";

    /// <summary>
    /// The number of tasks associated with the workspace.
    /// </summary>
    /// <remarks>
    /// This property is genereted by counting tasks in <see cref="Workspace.Tasks">Workspace.Tasks</see>
    /// </remarks>
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
