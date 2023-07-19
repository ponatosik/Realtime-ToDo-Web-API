using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Hubs;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;

namespace Realtime_ToDo_Web_API.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkspacesController : ControllerBase
{
    private readonly TodoListService _todoListService;
    private readonly WorkspaceRoomManager _workspaceRoomManager;
    private readonly IHubContext<WorkspacesHub, IWorkspacesClient> _hubContext;

    public WorkspacesController(TodoListService todoListService,
                                WorkspaceRoomManager workspaceRoomManager,
                                IHubContext<WorkspacesHub, IWorkspacesClient> hubContext)
    {
        _todoListService = todoListService;
        _workspaceRoomManager = workspaceRoomManager;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Loads all workspaces
    /// </summary>
    /// <response code="200">Returns all workspaces</response>
    [HttpGet]
    public IEnumerable<WorkspaceInfo> Get()
    {
        return _todoListService.GetWorkspacesInfo();
    }

    /// <summary>
    /// Loads single workspace
    /// </summary>
    /// <response code="200">Returns the workspace</response>
    /// <response code="404">If the workspace is not found</response>
    [HttpGet("{workspaceId}")]
    public ActionResult<WorkspaceInfo> Get(int workspaceId)
    {
        WorkspaceInfo? workspace = _todoListService.GetWorkspaceInfo(workspaceId);
        if (workspace == null) return NotFound($"Workspace with id {workspaceId} not found");
        return Ok(workspace!);
    }

    /// <summary>
    /// Creates a workspace
    /// </summary>
    /// <response code="201">Returns created workspace and path to it</response>
    /// <response code="404">If the workspace is not found</response>
    [HttpPost]
    public async Task<ActionResult<WorkspaceInfo>> Post(string worskspaceName)
    {
        WorkspaceInfo createdWorkspace = await _todoListService.AddWorkspace(worskspaceName);
        var routeValues = new
        {
            workspaceId = createdWorkspace.Id,
        };

        await _hubContext.Clients.All.AddWorkspace(createdWorkspace);
        return CreatedAtAction(nameof(Get), routeValues, createdWorkspace);
    }

    /// <summary>
    /// Updates a workspace
    /// </summary>
    /// <response code="200">Returns updated workspace</response>
    /// <response code="404">If the workspace is not found</response>
    [HttpPut("{workspaceId}")]
    public async Task<ActionResult<WorkspaceInfo>> Put(int workspaceId, string newWorkspaceName)
    {
        WorkspaceInfo? updatedWorkspace = await _todoListService.UpdateWorkspaceInfo(workspaceId, workspace =>
        {
            workspace.Name = newWorkspaceName;
        });
        if (updatedWorkspace == null) return NotFound($"Workspace with id {workspaceId} not found");

        await _hubContext.Clients.All.UpdateWorkspaceName(updatedWorkspace.Id, updatedWorkspace.Name);
        return Ok(updatedWorkspace);
    }

    /// <summary>
    /// Deletes a workspace
    /// </summary>
    /// <response code="200">Returns deleted workspace</response>
    /// <response code="404">If the workspace is not found</response>
    [HttpDelete("{workspaceId}")]
    public async Task<ActionResult<WorkspaceInfo>> Delete(int workspaceId)
    {
        WorkspaceInfo? targetWorkspace = _todoListService.GetWorkspaceInfo(workspaceId);
        if (targetWorkspace == null) return NotFound($"Workspace with id {workspaceId} not found");

        await _workspaceRoomManager.CloseWorkspaceRoomAsync(targetWorkspace.Id);
        await _hubContext.Clients.All.DeleteWorkspace(targetWorkspace.Id);
        WorkspaceInfo? deletedWorkspace = await _todoListService.DeleteWorkspace(workspaceId);

        return Ok(deletedWorkspace);
    }
}
