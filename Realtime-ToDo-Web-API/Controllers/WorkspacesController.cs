using Microsoft.AspNetCore.Mvc;
using Realtime_ToDo_Web_API.Data;
using Realtime_ToDo_Web_API.Models;

namespace Realtime_ToDo_Web_API.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkspacesController : ControllerBase
{
    private readonly TodoListContext _todoListContext;
    public WorkspacesController(TodoListContext todoListContext)
    {
        _todoListContext = todoListContext;
    }

    [HttpGet(Name = "GetWorkspaces")]
    public IEnumerable<Workspace> Get()
    {
        return _todoListContext.Workspaces;
    }

    [HttpPut(Name = "PutWorkspaces")]
    public async Task<Workspace> PutAsync(string worspaceName)
    {
        Workspace workspace = new()
        {
            Name = worspaceName,
            Tasks = new List<TodoTask>()
        };
        await _todoListContext.Workspaces.AddAsync(workspace);
        await _todoListContext.SaveChangesAsync();
        return workspace;
    }
}
