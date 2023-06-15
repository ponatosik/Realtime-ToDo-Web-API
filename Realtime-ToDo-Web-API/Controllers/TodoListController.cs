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
                              WorkspaceRoomManager workspacesRoomManager)
    {
        _todoListService = todoListService;
        _workspacesRoom = workspacesRoomManager; 
    }

    /// <summary>
    /// Loads all task in a workspace
    /// </summary>
    /// <response code="200">Returns tasks in the workspace</response>
    /// <response code="404">If the workspace is not found</response>
    [HttpGet("{workspaceId}")]
    public ActionResult<IEnumerable<TodoTask>> Get(int workspaceId)
    {
        IEnumerable<TodoTask>? tasks = _todoListService.GetWorkspaceTasks(workspaceId)?.OrderBy(task => task.Order);
        if (tasks == null) return NotFound($"Workspace with id {workspaceId} not found");
        return Ok(tasks!);
    }

    /// <summary>
    /// Finds single task in a workspace
    /// </summary>
    /// <response code="200">Returns the task</response>
    /// <response code="404">If the task is not found</response>
    [HttpGet("{workspaceId}/{taskId}")]
    public ActionResult<TodoTask> Get(int workspaceId, int taskId)
    {
        TodoTask? task = _todoListService.GetTask(workspaceId, taskId);
        if (task == null) return NotFound($"Task with id {taskId} not found in workspace with id {workspaceId}");
        return Ok(task!);
    }

    /// <summary>
    /// Creates new task in a workspace
    /// </summary>
    /// <response code="201">Returns created task and path to it</response>
    /// <response code="404">If the workspace is not found</response>
    [HttpPut("{workspaceId}")]
    public async Task<ActionResult<TodoTask>> Put(TodoTask task, int workspaceId)
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

    /// <summary>
    /// Deletes a task in a workspace
    /// </summary>
    /// <response code="200">Returns deleted task</response>
    /// <response code="404">If the workspace is not found</response>
    [HttpDelete("{workspaceId}/{taskId}")]
    public async Task<ActionResult<TodoTask>> Delete(int workspaceId, int taskId)
    {
        TodoTask? deletedTask = await _todoListService.DeleteTask(workspaceId, taskId);
        if (deletedTask == null) return NotFound($"Task with id {taskId} not found in workspace with id {workspaceId}");

        await _workspacesRoom.Clients(workspaceId).DeleteTask(deletedTask.Id);
        return Ok(deletedTask);
    }

    /// <summary>
    /// Updates a TodoTask in a workspace
    /// </summary>
    /// <response code="200">Returns updated task</response>
    /// <response code="404">If the workspace is not found</response>
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