using Microsoft.AspNetCore.Mvc;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;

namespace Realtime_ToDo_Web_API.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkspacesController : ControllerBase
{
    private readonly TodoListService _todoListService;
    private readonly ConnectionManager _connectionManager;

    public WorkspacesController(TodoListService todoListService, ConnectionManager connectionManager)
    {
        _todoListService = todoListService;
        _connectionManager = connectionManager;
    }

    [HttpGet]
    public IEnumerable<WorkspaceInfo> Get()
    {
        return _todoListService.GetWorkspacesInfo();
    }

    [HttpGet("{workspaceId}")]
    public ActionResult<WorkspaceInfo> Get(int workspaceId)
    {
        WorkspaceInfo? workspace = _todoListService.GetWorkspaceInfo(workspaceId);
        if (workspace == null) return NotFound($"Workspace with id {workspaceId} not found");
        return Ok(workspace!);
    }

    [HttpPost]
    public async Task<ActionResult<WorkspaceInfo>> Post(string worskspaceName)
    {
        WorkspaceInfo createdWorkspace = await _todoListService.AddWorkspace(worskspaceName);
        var routeValues = new
        {
            workspaceId = createdWorkspace.Id,
        };
        return CreatedAtAction(nameof(Get), routeValues, createdWorkspace);
    }

    [HttpPatch("{workspaceId}")]
    public async Task<ActionResult<WorkspaceInfo>> Patch(int workspaceId, string newWorkspaceName)
    {
        WorkspaceInfo? updatedWorkspace = await _todoListService.UpdateWorkspaceInfo(workspaceId, workspace =>
        {
            workspace.Name = newWorkspaceName;
        });
        if (updatedWorkspace == null) return NotFound($"Workspace with id {workspaceId} not found");
        return Ok(updatedWorkspace);
    }

    [HttpDelete("{workspaceId}")]
    public async Task<ActionResult<WorkspaceInfo>> Delete(int workspaceId)
    {
        WorkspaceInfo? deletedWorkspace = await _todoListService.DeleteWorkspace(workspaceId);
        if (deletedWorkspace == null) return NotFound($"Workspace with id {workspaceId} not found");
        if (_connectionManager.GetConnectedUsers(workspaceId) > 0)
            return Conflict($"Workspace with id {workspaceId} is currently used by other users. Try again later");
        return Ok(deletedWorkspace);
    }
}
