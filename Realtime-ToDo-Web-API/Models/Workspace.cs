namespace Realtime_ToDo_Web_API.Models;

/// <summary>
/// Represents a workspace containing a list of tasks.
/// </summary>
/// <remarks>
/// This model represents how workspace is stored in database.
/// To see how workspace is represented to user see <see cref="WorkspaceInfo"/>
/// </remarks>
public class Workspace
{
    /// <summary>
    /// The unique identifier of the workspace.
    /// </summary>
    /// <remarks>
    /// Cannot be changed by user
    /// </remarks>
    public int Id { get; set; }

    /// <summary>
    /// The name of the workspace.
    /// </summary>
    public string Name { get; set; } = "New workspace";

    /// <summary>
    /// The list of tasks associated with the workspace.
    /// </summary>
    public List<TodoTask>? Tasks { get; set; }
}
