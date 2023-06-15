using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Hubs;
using Realtime_ToDo_Web_API.Models;
using System.Collections.Generic;

namespace Realtime_ToDo_Web_API.Services.SignalR;

public class WorkspaceRoomManager
{
    private readonly Dictionary<string, int> _connectionIdByWorkpaceId;
    private readonly IHubContext<TodoListHub, ITodoListClient> _hubContext;
    public WorkspaceRoomManager(IHubContext<TodoListHub, ITodoListClient> todoListHub)
    {
        _connectionIdByWorkpaceId = new Dictionary<string, int>();
        _hubContext = todoListHub;
    }

    public IWorkspaceRoom GetWorkspaceRoom(HubCallerContext context)
    {
        return new WorkspaceRoom(context.ConnectionId, _hubContext, this);
    }

    public int GetWorkspaceId(string connectionId)
    {
        return _connectionIdByWorkpaceId[connectionId];
    }
    public int GetConnectedUsers(int workspaceId)
    {
        return _connectionIdByWorkpaceId.Values.Count(storedWorkspaceId => storedWorkspaceId == workspaceId);
    }
    public bool IsConnected(string connectionId)
    {
        return _connectionIdByWorkpaceId.ContainsKey(connectionId);
    }
    public async Task ConnectAsync(string connectionId, int workspaceId)
    {
        await _hubContext.Groups.AddToGroupAsync(connectionId, GenerateGroupName(workspaceId));
        await _hubContext.Clients.Client(connectionId).Connected(workspaceId);
        _connectionIdByWorkpaceId.Add(connectionId, workspaceId);
    }
    public async Task DisconnectAsync(string connectionId)
    {
        int workspaceId = GetWorkspaceId(connectionId);
        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, GenerateGroupName(workspaceId));
        await _hubContext.Clients.Client(connectionId).Disconnected(workspaceId);
        _connectionIdByWorkpaceId.Remove(connectionId);
    }
    public async Task CloseWorkspaceRoomAsync(int workspaceId)
    {
        foreach (var connection in _connectionIdByWorkpaceId.Where(pair => pair.Value == workspaceId))
            await DisconnectAsync(connection.Key);
    }

    public ITodoListClient Clients(int workspaceId) 
    {
        return _hubContext.Clients.Group(GenerateGroupName(workspaceId));
    }
    public string GenerateGroupName(int workspaceId)
    {
        return $"Workspace group: {workspaceId}";
    } 
}
