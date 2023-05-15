using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Hubs;

namespace Realtime_ToDo_Web_API.Services.SignalR;

public class ConnectionManager
{
    private readonly Dictionary<string, int> _connectionIdByWorkpaceId;
    private readonly IHubContext<TodoListHub, ITodoListClient> _hubContext;
    public ConnectionManager(IHubContext<TodoListHub, ITodoListClient> todoListHub)
    {
        _connectionIdByWorkpaceId = new Dictionary<string, int>();
        _hubContext = todoListHub;
    }

    public IWorkspaceConnection GetWorkspaceConnection(HubCallerContext context)
    {
        return new WorkspaceConnection(context, _hubContext, this);
    }

    public int GetWorkspaceId(string connectionId)
    {
        return _connectionIdByWorkpaceId[connectionId];
    }
    public int GetConnectedUsers(int workspaceId)
    {
        return _connectionIdByWorkpaceId.Values.Count(storedWorkspaceId => storedWorkspaceId == workspaceId);
    }
    public bool HasConnection(string connectionId)
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
}
