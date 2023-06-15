using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;


namespace Realtime_ToDo_Web_API.Hubs;

public class TodoListHub : Hub<ITodoListClient>
{
    private readonly TodoListService _todoListService;
    private readonly WorkspaceRoomManager _workspaceRoomManager;
    public TodoListHub(TodoListService todoListService, WorkspaceRoomManager workspaceRoomManager)
    {
        _todoListService = todoListService;
        _workspaceRoomManager = workspaceRoomManager;
    }

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

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (WorkspaceRoom.IsConnected)
            DisconnectFromWorkspace().Wait();

        return base.OnDisconnectedAsync(exception);
    }

    protected IWorkspaceRoom WorkspaceRoom
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