using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;


namespace Realtime_ToDo_Web_API.Hubs;

/// <summary>
/// Represents the hub for managing todo tasks in workspaces.
/// Notifies <see cref="ITodoListClient"/> about changes in todo tasks
/// </summary>
public class TodoListHub : Hub<ITodoListClient>
{
    private readonly TodoListService _todoListService;
    private readonly WorkspaceRoomManager _workspaceRoomManager;
    public TodoListHub(TodoListService todoListService, WorkspaceRoomManager workspaceRoomManager)
    {
        _todoListService = todoListService;
        _workspaceRoomManager = workspaceRoomManager;
    }

    /// <summary>
    /// Add new task to the connected workspace.
    /// </summary>
    /// <param name="title">The title of the task.</param>
    /// <param name="deadline">The deadline of the task.</param>
    public async Task AddTask(string title, DateTime? deadline)
    {
        if (!WorkspaceRoom.IsConnected) 
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask task = new TodoTask
        {
            Title = title,
            Deadline = deadline
        };

        await _todoListService.AddTask(WorkspaceRoom.WorkspaceId, task);
        await WorkspaceRoom.Clients.AddTask(task);
    }

    /// <summary>
    /// Update an existing task in the connected workspace.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="newTask">New task object.</param>
    public async Task UpdateTask(int taskId, TodoTask newTask)
    {
        if (!WorkspaceRoom.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceRoom.WorkspaceId, taskId, (targetTask) => {
            targetTask.Title = newTask.Title;
            targetTask.Completed = newTask.Completed;
            targetTask.Deadline = newTask.Deadline;
            targetTask.Order = newTask.Order;
        });

        if (updatedTask == null) 
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceRoom.Clients.UpdateTask(updatedTask);
    }

    /// <summary>
    /// Delete a task from the connected workspace.
    /// </summary>
    /// <param name="taskId">The ID of the task to delete.</param>
    public async Task DeleteTask(int taskId)
    {
        if (!WorkspaceRoom.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? deletedTask = await _todoListService.DeleteTask(WorkspaceRoom.WorkspaceId, taskId);
        if (deletedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceRoom.Clients.DeleteTask(taskId);
    }

    /// <summary>
    /// Update the title of a task in the connected workspace.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="newTitle">The new title of the task.</param>
    public async Task UpdateTaskTitle(int taskId, string newTitle)
    {
        if (!WorkspaceRoom.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceRoom.WorkspaceId, taskId, (targetTask) => {
            targetTask.Title = newTitle;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceRoom.Clients.UpdateTaskTitle(taskId, updatedTask.Title);
    }

    /// <summary>
    /// Update the completion status of a task in the connected workspace.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="newCompleted">The new completion status of the task.</param>
    public async Task UpdateTaskCompleted(int taskId, bool newCompleted)
    {
        if (!WorkspaceRoom.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceRoom.WorkspaceId, taskId, (targetTask) => {
            targetTask.Completed = newCompleted;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceRoom.Clients.UpdateTaskCompleted(taskId, updatedTask.Completed);
    }

    /// <summary>
    /// Update the deadline of a task in the connected workspace.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="newDeadline">The new deadline of the task.</param>
    public async Task UpdateTaskDeadline(int taskId, DateTime newDeadline)
    {
        if (!WorkspaceRoom.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceRoom.WorkspaceId, taskId, (targetTask) => {
            targetTask.Deadline = newDeadline;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceRoom.Clients.UpdateTaskDeadline(taskId, updatedTask.Deadline);
    }

    /// <summary>
    /// Update the order of a task in the connected workspace.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="destinationOrder">The new order of the task.</param>
    public async Task UpdateTaskOrder(int taskId, int destinationOrder)
    {
        if (!WorkspaceRoom.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceRoom.WorkspaceId, taskId, (targetTask) => {
            targetTask.Order = destinationOrder;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceRoom.Clients.UpdateTaskOrder(taskId, updatedTask.Order);
    }

    /// <summary>
    /// Connect to a workspace with the specified ID.
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace to connect to.</param>
    /// <remarks>
    /// If you alredy connected to a workspace, you will be automatiacally disconnected from it
    /// </remarks>
    public async Task ConnectToWorkspace(int workspaceId)
    {
        WorkspaceInfo? targetWorkspace = _todoListService.GetWorkspaceInfo(workspaceId);
        if (targetWorkspace == null)
        {
            await Clients.Caller.Error($"Workspace with id {workspaceId} does not exist");
            return;
        }

        if (WorkspaceRoom.IsConnected)
        {
            await WorkspaceRoom.Clients.UserDisconnected(WorkspaceRoom.ConnectedUsersCount - 1);
            await WorkspaceRoom.DisconnectAsync();
        }

        await WorkspaceRoom.ConnectAsync(workspaceId);
        await WorkspaceRoom.Clients.UserConnected(WorkspaceRoom.ConnectedUsersCount);
    }

    /// <summary>
    /// Disconnect from the currently connected workspace.
    /// </summary>
    public async Task DisconnectFromWorkspace()
    {
        if (!WorkspaceRoom.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        await WorkspaceRoom.Clients.UserDisconnected(WorkspaceRoom.ConnectedUsersCount - 1);
        await WorkspaceRoom.DisconnectAsync();
    }

    /// <summary>
    /// Called when a connection is established with the hub.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Clients should not try to call this method, as it is called automatically.
    /// </para>
    /// <para>
    ///     While you are connecting to the hub, you can specify workspaceid query string
    ///     to automatically connect to specified workspace.
    /// </para>
    /// </remarks>
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext != null && httpContext.Request.Query.Keys.Contains("workspaceid"))
        {
            string workspaceIdString = httpContext.Request.Query["workspaceid"]!;
            if (int.TryParse(workspaceIdString, out int workspaceId))
            {
                await ConnectToWorkspace(workspaceId);
            }
        }
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a connection to the hub is terminated.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnection.</param>
    /// <remarks>
    /// Clients should not try to call this method, as it is called automatically
    /// </remarks>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (WorkspaceRoom.IsConnected)
            await DisconnectFromWorkspace();

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Lazy-loaded property for IWorkspaceRoom with hub context
    /// </summary>
    private IWorkspaceRoom WorkspaceRoom
    {
        get
        {
            if (_workspaceRoom != null) return _workspaceRoom;
            _workspaceRoom = _workspaceRoomManager.GetWorkspaceRoom(Context);
            return _workspaceRoom;
        }
    }
    private IWorkspaceRoom? _workspaceRoom;
}