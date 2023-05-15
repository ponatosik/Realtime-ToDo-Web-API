using Realtime_ToDo_Web_API.Hubs;

namespace Realtime_ToDo_Web_API.Services.SignalR;

public interface IWorkspaceConnection
{
    public int ConnectedUsersCount { get; }
    public bool IsConnected { get; }
    public int WorkspaceId { get; }
    public ITodoListClient Group { get; }

    public void Connect(int workspaceId);
    public void Disconnect();
}
