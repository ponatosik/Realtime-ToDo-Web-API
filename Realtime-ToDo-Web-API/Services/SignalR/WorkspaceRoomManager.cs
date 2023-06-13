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
        return new WorkspaceRoom(context, _hubContext, this);
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
    public void Connect(string connectionId, int workspaceId)
    {
        _connectionIdByWorkpaceId.Add(connectionId, workspaceId);
    }
    public void Disconnect(string connectionId)
    {
        _connectionIdByWorkpaceId.Remove(connectionId);
    }
    public void CloseWorkspaceRoom(int workspaceId)
    {
        foreach (var connection in _connectionIdByWorkpaceId.Where(pair => pair.Value == workspaceId))
            Disconnect(connection.Key);
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
