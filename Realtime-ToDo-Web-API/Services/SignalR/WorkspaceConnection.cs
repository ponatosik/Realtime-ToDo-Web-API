using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Hubs;

namespace Realtime_ToDo_Web_API.Services.SignalR;

public class WorkspaceConnection : IWorkspaceConnection
{
    private readonly HubCallerContext _callerContext;
    private readonly IHubContext<TodoListHub, ITodoListClient> _hubContext;
    private readonly ConnectionManager _manager;

    public WorkspaceConnection(HubCallerContext callerContext, IHubContext<TodoListHub, ITodoListClient> hubContext, ConnectionManager manager)
    {
        _callerContext = callerContext;
        _hubContext = hubContext;
        _manager = manager;
    }

    public int ConnectedUsersCount => _manager.GetConnectedUsers(WorkspaceId);
    public bool IsConnected => _manager.HasConnection(_callerContext.ConnectionId);
    public int WorkspaceId => _manager.GetWorkspaceId(_callerContext.ConnectionId);

    public ITodoListClient Group => _hubContext.Clients.Group(GroupName(WorkspaceId));

    public void Connect(int workspaceId)
    {
        if (IsConnected) Disconnect();
        _manager.Connect(_callerContext.ConnectionId, workspaceId);
        _hubContext.Groups.AddToGroupAsync(_callerContext.ConnectionId, GroupName(workspaceId));
    }
    public void Disconnect()
    {
        if (!IsConnected) return;

        _hubContext.Groups.RemoveFromGroupAsync(_callerContext.ConnectionId, GroupName(WorkspaceId)).Wait();
        _manager.Disconnect(_callerContext.ConnectionId);
    }

    private string GroupName(int workspaceId) => $"Workspace group: {workspaceId}";
}