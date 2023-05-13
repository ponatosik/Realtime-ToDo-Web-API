using Microsoft.AspNetCore.Mvc;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;

namespace Realtime_ToDo_Web_API.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoListController : ControllerBase
{
    private readonly TodoListService _todoListService;
    public TodoListController(TodoListService todoListService)
    {
        _todoListService = todoListService;
    }

    [HttpGet("{workspaceId}", Name = "GetTodoListworkspaceId")]
    public IEnumerable<TodoTask>? Get(int workspaceId)
    {
        return _todoListService.GetWorkspaceTasks(workspaceId)?.OrderBy(task => task.Id);
    }

    [HttpPut("{workspaceId}", Name = "PutTodoListByworkspaceId")]
    public async Task<TodoTask?> PutAsync(TodoTask task, int workspaceId)
    {
        return await _todoListService.AddTask(workspaceId, task);
    }
}