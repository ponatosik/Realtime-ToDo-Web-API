using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;


namespace Realtime_ToDo_Web_API.Hubs;

/// <summary>
/// Represents the hub for managing workspaces.
/// Notifies <see cref="IWorkspacesClient"/> about changes in workspaces
/// </summary>
public class WorkspacesHub : Hub<IWorkspacesClient>
{
    private readonly TodoListService _todoListService;
    private readonly WorkspaceRoomManager _workspaceRoomManager;
    public WorkspacesHub(TodoListService TodoListService, WorkspaceRoomManager connectionManager)
    {
        _todoListService = TodoListService;
        _workspaceRoomManager = connectionManager;
    }

    /// <summary>
    /// Add a new workspace with the specified name.
    /// </summary>
    /// <param name="workspaceName">The name of the workspace to add.</param>
    public async Task AddWorkspace(string workspaceName)
    {
        WorkspaceInfo workspace = await _todoListService.AddWorkspace(workspaceName);
        await Clients.All.AddWorkspace(workspace);
    }

    /// <summary>
    /// Update the name of a workspace with the specified ID.
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace to update.</param>
    /// <param name="newName">The new name of the workspace.</param>
    public async Task UpdateWorkspaceName(int workspaceId, string newName)
    {
        WorkspaceInfo? updatedWorkspace = await _todoListService.UpdateWorkspaceInfo(workspaceId, (targetWorkspace) => {
            targetWorkspace.Name = newName;
        });

        if (updatedWorkspace == null) 
        {
            await Clients.Caller.Error($"Workspace with id {workspaceId} does not exist");
            return;
        }

        await Clients.All.UpdateWorkspaceName(workspaceId, updatedWorkspace.Name);
    }

    /// <summary>
    /// Delete a workspace with the specified ID.
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace to delete.</param>
    public async Task DeleteWorkspace(int workspaceId)
    {
        WorkspaceInfo? targetWorkspace = _todoListService.GetWorkspaceInfo(workspaceId);
        if (targetWorkspace == null)
        {
            await Clients.Caller.Error($"Task with id {workspaceId} does not exist");
            return;
        }

        await _workspaceRoomManager.CloseWorkspaceRoomAsync(workspaceId);
        await Clients.All.DeleteWorkspace(workspaceId);
        await _todoListService.DeleteWorkspace(workspaceId);
    }

}