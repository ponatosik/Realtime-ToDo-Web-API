using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;


namespace Realtime_ToDo_Web_API.Hubs;

public class TodoListHub : Hub<ITodoListClient>
{
    private readonly TodoListService _todoListService;
    private readonly ConnectionManager _connectionManager;
    public TodoListHub(TodoListService todoListService, ConnectionManager connectionManager)
    {
        _todoListService = todoListService;
        _connectionManager = connectionManager;
    }

    public async Task AddTask(string title, DateTime? deadline)
    {
        if (!WorkspaceConnection.IsConnected) 
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask task = new TodoTask
        {
            Title = title,
            Deadline = deadline
        };

        await _todoListService.AddTask(WorkspaceConnection.WorkspaceId, task);
        await WorkspaceConnection.Group.AddTask(task);
    }

    public async Task UpdateTask(int taskId, TodoTask newTask)
    {
        if (!WorkspaceConnection.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceConnection.WorkspaceId, taskId, (targetTask) => {
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

        await WorkspaceConnection.Group.UpdateTask(updatedTask);
    }

    public async Task DeleteTask(int taskId)
    {
        if (!WorkspaceConnection.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? deletedTask = await _todoListService.DeleteTask(WorkspaceConnection.WorkspaceId, taskId);
        if (deletedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceConnection.Group.DeleteTask(taskId);
    }

    public async Task UpdateTaskTitle(int taskId, string newTitle)
    {
        if (!WorkspaceConnection.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceConnection.WorkspaceId, taskId, (targetTask) => {
            targetTask.Title = newTitle;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceConnection.Group.UpdateTaskTitle(taskId, updatedTask.Title);
    }

    public async Task UpdateTaskCompleted(int taskId, bool newCompleted)
    {
        if (!WorkspaceConnection.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceConnection.WorkspaceId, taskId, (targetTask) => {
            targetTask.Completed = newCompleted;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceConnection.Group.UpdateTaskCompleted(taskId, updatedTask.Completed);
    }

    public async Task UpdateTaskDeadline(int taskId, DateTime newDeadline)
    {
        if (!WorkspaceConnection.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceConnection.WorkspaceId, taskId, (targetTask) => {
            targetTask.Deadline = newDeadline;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceConnection.Group.UpdateTaskDeadline(taskId, updatedTask.Deadline);
    }

    public async Task UpdateTaskOrder(int taskId, int destinationOrder)
    {
        if (!WorkspaceConnection.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        TodoTask? updatedTask = await _todoListService.UpdateTask(WorkspaceConnection.WorkspaceId, taskId, (targetTask) => {
            targetTask.Order = destinationOrder;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await WorkspaceConnection.Group.UpdateTaskOrder(taskId, updatedTask.Order);
    }

    public async Task ConnectToWorkspace(int workspaceId)
    {
        Workspace? targetWorkspace = _todoListService.GetWorkspaceInfo(workspaceId);
        if (targetWorkspace == null)
        {
            await Clients.Caller.Error($"Workspace with id {workspaceId} does not exist");
            return;
        }

        if (WorkspaceConnection.IsConnected)
        {
            await WorkspaceConnection.Group.UserDisconnected(WorkspaceConnection.ConnectedUsersCount - 1);
            WorkspaceConnection.Disconnect();
        } 

        WorkspaceConnection.Connect(workspaceId);
        await WorkspaceConnection.Group.UserConnected(WorkspaceConnection.ConnectedUsersCount);
    }

    public async Task DisconnectFromWorkspace()
    {
        if (!WorkspaceConnection.IsConnected)
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        await WorkspaceConnection.Group.UserDisconnected(WorkspaceConnection.ConnectedUsersCount - 1);
        WorkspaceConnection.Disconnect();
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
        if (WorkspaceConnection.IsConnected)
            DisconnectFromWorkspace().Wait();

        return base.OnDisconnectedAsync(exception);
    }

    protected IWorkspaceConnection WorkspaceConnection
    {
        get
        {
            if (_workspaceConnection != null) return _workspaceConnection;
            _workspaceConnection = _connectionManager.GetWorkspaceConnection(Context);
            return _workspaceConnection;
        }
    }
    private IWorkspaceConnection? _workspaceConnection;
}