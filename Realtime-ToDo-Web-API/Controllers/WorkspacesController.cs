using Microsoft.AspNetCore.Mvc;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;

namespace Realtime_ToDo_Web_API.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkspacesController : ControllerBase
{
    private readonly TodoListService _todoListService;
    public WorkspacesController(TodoListService todoListService)
    {
        _todoListService = todoListService;
    }

    [HttpGet(Name = "GetWorkspaces")]
    public IEnumerable<WorkspaceInfo> Get()
    {
        return _todoListService.GetWorkspacesInfo();
    }

    [HttpPut(Name = "PutWorkspaces")]
    public async Task<WorkspaceInfo> PutAsync(string worskspaceName)
    {
        return await _todoListService.AddWorkspace(worskspaceName);
    }
}
