using System.Collections;
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
    public ActionResult<IEnumerable<TodoTask>> Get(int workspaceId)
    {
        IEnumerable<TodoTask>? tasks = _todoListService.GetWorkspaceTasks(workspaceId)?.OrderBy(task => task.Order);
        if (tasks == null) return NotFound($"Workspace with id {workspaceId} not found");
        return Ok(tasks!);
    }

    [HttpPut("{workspaceId}", Name = "PutTodoListByworkspaceId")]
    public async Task<ActionResult<TodoTask>> Put(TodoTask task, int workspaceId)
    {
        TodoTask? createdTask = await _todoListService.AddTask(workspaceId, task);
        if (createdTask == null) return NotFound($"Workspace with id {workspaceId} not found");
        return Ok(createdTask);
    }
}