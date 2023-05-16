using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;


namespace Realtime_ToDo_Web_API.Hubs;

public class WorkspacesHub : Hub<IWorkspacesClient>
{
    private readonly TodoListService _TodoListService;
    private readonly ConnectionManager _connectionManager;
    public WorkspacesHub(TodoListService TodoListService, ConnectionManager connectionManager)
    {
        _TodoListService = TodoListService;
        _connectionManager = connectionManager;
    }

    public async Task AddWorkspace(string workspaceName)
    {
        Workspace workspace = await _TodoListService.AddWorkspace(workspaceName);
        await Clients.All.AddWorkspace(workspace);
    }

    public async Task UpdateWorkspaceName(int workspaceId, string newName)
    {
        Workspace? updatedWorkspace = await _TodoListService.UpdateWorkspaceInfo(workspaceId, (targetWorkspace) => {
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
        Workspace? deletedWorkspace = await _TodoListService.DeleteWorkspace(workspaceId);
        if (deletedWorkspace == null)
        {
            await Clients.Caller.Error($"Task with id {workspaceId} does not exist");
            return;
        }

        _connectionManager.DisconnectFromWorkspaceAll(workspaceId);
        await Clients.All.DeleteWorkspace(workspaceId);
    }

}