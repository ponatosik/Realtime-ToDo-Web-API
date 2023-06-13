using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Hubs;

namespace Realtime_ToDo_Web_API.Services.SignalR;

public class WorkspaceRoom : IWorkspaceRoom
{
    private readonly HubCallerContext _callerContext;
    private readonly IHubContext<TodoListHub, ITodoListClient> _hubContext;
    private readonly WorkspaceRoomManager _rooomManager;

    public WorkspaceRoom(HubCallerContext callerContext, IHubContext<TodoListHub, ITodoListClient> hubContext, WorkspaceRoomManager manager)
    {
        _callerContext = callerContext;
        _hubContext = hubContext;
        _rooomManager = manager;
    }

    public int ConnectedUsersCount => _rooomManager.GetConnectedUsers(WorkspaceId);
    public bool IsConnected => _rooomManager.IsConnected(_callerContext.ConnectionId);
    public int WorkspaceId => _rooomManager.GetWorkspaceId(_callerContext.ConnectionId);

    public ITodoListClient Clients => _rooomManager.Clients(WorkspaceId);

    public void Connect(int workspaceId)
    {
        if (IsConnected) Disconnect();
        _rooomManager.Connect(_callerContext.ConnectionId, workspaceId);
        _hubContext.Groups.AddToGroupAsync(_callerContext.ConnectionId, GroupName(workspaceId));
    }
    public void Disconnect()
    {
        if (!IsConnected) return;

        _hubContext.Groups.RemoveFromGroupAsync(_callerContext.ConnectionId, GroupName(WorkspaceId)).Wait();
        _rooomManager.Disconnect(_callerContext.ConnectionId);
    }

    private string GroupName(int workspaceId) => _rooomManager.GenerateGroupName(workspaceId);
}