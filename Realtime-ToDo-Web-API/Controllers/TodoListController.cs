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

    [HttpGet("{workspaceId}")]
    public ActionResult<IEnumerable<TodoTask>> Get(int workspaceId)
    {
        IEnumerable<TodoTask>? tasks = _todoListService.GetWorkspaceTasks(workspaceId)?.OrderBy(task => task.Order);
        if (tasks == null) return NotFound($"Workspace with id {workspaceId} not found");
        return Ok(tasks!);
    }

    [HttpGet("{workspaceId}/{taskId}")]
    public ActionResult<TodoTask> Get(int workspaceId, int taskId)
    {
        TodoTask? task = _todoListService.GetTask(workspaceId, taskId);
        if (task == null) return NotFound($"Task with id {taskId} not found in workspace with id {workspaceId}");
        return Ok(task!);
    }

    [HttpPost("{workspaceId}")]
    public async Task<ActionResult<TodoTask>> Post(TodoTask task, int workspaceId)
    {
        TodoTask? createdTask = await _todoListService.AddTask(workspaceId, task);
        if (createdTask == null) return NotFound($"Workspace with id {workspaceId} not found");
        var routeValues = new
        {
            workspaceId = workspaceId,
            taskId = createdTask.Id
        };
        return CreatedAtAction(nameof(Get), routeValues, createdTask);
    }

    [HttpDelete("{workspaceId}/{taskId}")]
    public async Task<ActionResult<TodoTask>> Delete(TodoTask task, int workspaceId, int taskId)
    {
        TodoTask? deletedTask = await _todoListService.DeleteTask(workspaceId, taskId);
        if (deletedTask == null) return NotFound($"Task with id {taskId} not found in workspace with id {workspaceId}");
        return Ok(deletedTask);
    }

    [HttpPut("{workspaceId}")]
    public async Task<ActionResult<TodoTask>> Put(TodoTask newTask, int workspaceId)
    {
        var taskId = newTask.Id;
        TodoTask? updatedTask = await _todoListService.UpdateTask(workspaceId, taskId, task =>
        {
            task = newTask;
        });
        if (updatedTask == null) return NotFound($"Task with id {taskId} not found in workspace with id {workspaceId}");
        return Ok(updatedTask);
    }
}