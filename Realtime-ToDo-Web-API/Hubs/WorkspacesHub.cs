using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;


namespace Realtime_ToDo_Web_API.Hubs;

public class WorkspacesHub : Hub<IWorkspacesClient>
{
    private readonly TodoListService _todoListService;
    private readonly WorkspaceRoomManager _workspaceRoomManager;
    public WorkspacesHub(TodoListService TodoListService, WorkspaceRoomManager connectionManager)
    {
        _todoListService = TodoListService;
        _workspaceRoomManager = connectionManager;
    }

    public async Task AddWorkspace(string workspaceName)
    {
        WorkspaceInfo workspace = await _todoListService.AddWorkspace(workspaceName);
        await Clients.All.AddWorkspace(workspace);
    }

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

    public async Task DeleteWorkspace(int workspaceId)
    {
        WorkspaceInfo? deletedWorkspace = await _todoListService.DeleteWorkspace(workspaceId);
        if (deletedWorkspace == null)
        {
            await Clients.Caller.Error($"Task with id {workspaceId} does not exist");
            return;
        }

        _workspaceRoomManager.CloseWorkspaceRoom(workspaceId);
        await Clients.All.DeleteWorkspace(workspaceId);
    }

}