using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Hubs;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;

namespace Realtime_ToDo_Web_API.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoListController : ControllerBase
{
    private readonly TodoListService _todoListService;
    private readonly WorkspaceRoomManager _workspacesRoom;


    public TodoListController(TodoListService todoListService,
                              WorkspaceRoomManager workspacesRoomManager,
                              IHubContext<TodoListHub, ITodoListClient> hubContext)
    {
        _todoListService = todoListService;
        _workspacesRoom = workspacesRoomManager; 
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

        await _workspacesRoom.Clients(workspaceId).AddTask(createdTask);
        return CreatedAtAction(nameof(Get), routeValues, createdTask);
    }

    [HttpDelete("{workspaceId}/{taskId}")]
    public async Task<ActionResult<TodoTask>> Delete(int workspaceId, int taskId)
    {
        TodoTask? deletedTask = await _todoListService.DeleteTask(workspaceId, taskId);
        if (deletedTask == null) return NotFound($"Task with id {taskId} not found in workspace with id {workspaceId}");

        await _workspacesRoom.Clients(workspaceId).DeleteTask(deletedTask.Id);
        return Ok(deletedTask);
    }

    [HttpPatch("{workspaceId}")]
    public async Task<ActionResult<TodoTask>> Patch(TodoTask newTask, int workspaceId)
    {
        var taskId = newTask.Id;
        TodoTask? updatedTask = await _todoListService.UpdateTask(workspaceId, taskId, task =>
        {
            task.Title = newTask.Title;
            task.Completed = newTask.Completed;
            task.Deadline = newTask.Deadline;
            task.Order = newTask.Order;
        });
        if (updatedTask == null) return NotFound($"Task with id {taskId} not found in workspace with id {workspaceId}");

        await _workspacesRoom.Clients(workspaceId).UpdateTask(updatedTask);
        return Ok(updatedTask);
    }
}