using Realtime_ToDo_Web_API.Hubs;

namespace Realtime_ToDo_Web_API.Services.SignalR;

public interface IWorkspaceRoom
{
    public int ConnectedUsersCount { get; }
    public bool IsConnected { get; }
    public int WorkspaceId { get; }
    public ITodoListClient Clients { get; }

    public void Connect(int workspaceId);
    public void Disconnect();
}
