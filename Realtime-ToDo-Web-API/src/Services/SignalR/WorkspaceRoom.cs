using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Hubs;

namespace Realtime_ToDo_Web_API.Services.SignalR;

public class WorkspaceRoom : IWorkspaceRoom
{
    private readonly string _connectionId;
    private readonly IHubContext<TodoListHub, ITodoListClient> _hubContext;
    private readonly WorkspaceRoomManager _rooomManager;

    public WorkspaceRoom(string connectionId, IHubContext<TodoListHub, ITodoListClient> hubContext, WorkspaceRoomManager manager)
    {
        _connectionId = connectionId;
        _hubContext = hubContext;
        _rooomManager = manager;
    }

    public int ConnectedUsersCount => _rooomManager.GetConnectedUsers(WorkspaceId);
    public bool IsConnected => _rooomManager.IsConnected(_connectionId);
    public int WorkspaceId => _rooomManager.GetWorkspaceId(_connectionId);

    public ITodoListClient Clients => _rooomManager.Clients(WorkspaceId);

    public async Task ConnectAsync(int workspaceId)
    {
        if (IsConnected) await DisconnectAsync();
        await _rooomManager.ConnectAsync(_connectionId, workspaceId);
    }
    public async Task DisconnectAsync()
    {
        if (!IsConnected) return;
        await _hubContext.Groups.RemoveFromGroupAsync(_connectionId, GroupName(WorkspaceId));
        await _rooomManager.DisconnectAsync(_connectionId);
    }

    private string GroupName(int workspaceId) => _rooomManager.GenerateGroupName(workspaceId);
}