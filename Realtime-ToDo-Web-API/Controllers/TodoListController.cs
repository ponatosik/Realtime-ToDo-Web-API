using Microsoft.AspNetCore.Mvc;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Realtime_ToDo_Web_API.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoListController : ControllerBase
{
    private readonly TodoListContext _todoListContext;
    public TodoListController(TodoListContext todoListContext)
    {
        _todoListContext = todoListContext;
    }

    [HttpGet("{worspaceId}", Name = "GetTodoListByWorspaceId")]
    public IEnumerable<TodoTask>? Get(int worspaceId)
    {
        Workspace? workspace = _todoListContext.Workspaces.Include(workspace => workspace.Tasks).FirstOrDefault(workspace => workspace.Id == worspaceId);
        if (workspace == null) 
            return null;

        return workspace.Tasks.OrderBy(task => task.Order);
    }

    [HttpPut("{worspaceId}", Name = "PutTodoListByWorspaceId")]
    public async Task<TodoTask>? PutAsync(TodoTask task, int worspaceId)
    {
        Workspace? workspace = _todoListContext.Workspaces.Include(workspace => workspace.Tasks).FirstOrDefault(workspace => workspace.Id == worspaceId);
        if (workspace == null)
            return null;

        task.Id = default;
        task.Order = workspace.Tasks.Count();

        workspace.Tasks.Add(task);
        await _todoListContext.SaveChangesAsync();
        return task;
    }
}