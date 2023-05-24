using Realtime_ToDo_Web_API.Models;

namespace Realtime_ToDo_Web_API.Hubs;

public interface IWorkspacesClient
{
    Task AddWorkspace(WorkspaceInfo workspace);
    Task UpdateWorkspaceName(int workspaceId, string name);
    Task DeleteWorkspace(int workspaceId);

    Task Error(string errorMessage);
}
