using Realtime_ToDo_Web_API.Models;

namespace Realtime_ToDo_Web_API.Hubs;

/// <summary>
/// Represents the client interface for <see cref="WorkspacesHub">Workspaces SignalR hub</see>.
/// Defines about which actions in workspaces the client can be notified about
/// </summary>
/// <remarks>
/// Does not notify about chenges in <see cref="Workspace.Tasks">Workspace.Tasks</see> or <see cref="WorkspaceInfo.TaskCount">WorkspaceInfo.tasksCount</see>.
/// </remarks>
public interface IWorkspacesClient
{
    /// <summary>
    /// Notifies the client that a new workspace has been added.
    /// </summary>
    /// <param name="workspace">The information of the added workspace.</param>
    Task AddWorkspace(WorkspaceInfo workspace);
    /// <summary>
    /// Notifies the client that the name of a workspace has been updated.
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace.</param>
    /// <param name="name">The updated name of the workspace.</param>
    Task UpdateWorkspaceName(int workspaceId, string name);
    /// <summary>
    /// Notifies the client that a workspace has been deleted.
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace.</param>
    Task DeleteWorkspace(int workspaceId);

    /// <summary>
    /// Notifies the client about an error that occurred.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    Task Error(string errorMessage);
}
