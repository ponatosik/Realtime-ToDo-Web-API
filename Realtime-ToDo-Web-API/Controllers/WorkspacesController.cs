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
    public IEnumerable<Workspace> Get()
    {
        return _todoListService.GetWorkspaces();
    }

    [HttpPut(Name = "PutWorkspaces")]
    public async Task<Workspace> PutAsync(string worskspaceName)
    {
        return await _todoListService.AddWorkspace(worskspaceName);
    }
}
