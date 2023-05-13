using Microsoft.AspNetCore.SignalR;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Services;

namespace Realtime_ToDo_Web_API.Hubs;

public class TodoListHub : Hub<ITodoListClient>
{
    private readonly IUserConnectionStorage _connectedUsers;
    private readonly TodoListService _todoListService;
    public TodoListHub(TodoListService todoListService, IUserConnectionStorage connectedUsers)
    {
        _todoListService = todoListService;
        _connectedUsers = connectedUsers;
    }

    public async Task AddTask(string title, DateTime? deadline)
    {
        int? workspaceId = await GetConnectedWorkspace();
        if (workspaceId == null) return;

        TodoTask task = new TodoTask
        {
            Title = title,
            Deadline = deadline
        };

        await _todoListService.AddTask(workspaceId.Value, task);
        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).AddTask(task);
    }

    public async Task UpdateTask(int taskId, TodoTask newTask)
    {
        int? workspaceId = await GetConnectedWorkspace();
        if (workspaceId == null) return;

        TodoTask? updatedTask = await _todoListService.UpdateTask(workspaceId.Value, taskId, (targetTask) => {
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

        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).UpdateTask(updatedTask);
    }

    public async Task DeleteTask(int taskId)
    {
        int? workspaceId = await GetConnectedWorkspace();
        if (workspaceId == null) return;

        TodoTask? deletedTask = await _todoListService.DeleteTask(workspaceId.Value, taskId);
        if (deletedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).DeleteTask(taskId);
    }

    public async Task UpdateTaskTitle(int taskId, string newTitle)
    {
        int? workspaceId = await GetConnectedWorkspace();
        if (workspaceId == null) return;

        TodoTask? updatedTask = await _todoListService.UpdateTask(workspaceId.Value, taskId, (targetTask) => {
            targetTask.Title = newTitle;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).UpdateTaskTitle(taskId, updatedTask.Title);
    }

    public async Task UpdateTaskCompleted(int taskId, bool newCompleted)
    {
        int? workspaceId = await GetConnectedWorkspace();
        if (workspaceId == null) return;

        TodoTask? updatedTask = await _todoListService.UpdateTask(workspaceId.Value, taskId, (targetTask) => {
            targetTask.Completed = newCompleted;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).UpdateTaskCompleted(taskId, updatedTask.Completed);
    }

    public async Task UpdateTaskDeadline(int taskId, DateTime newDeadline)
    {
        int? workspaceId = await GetConnectedWorkspace();
        if (workspaceId == null) return;

        TodoTask? updatedTask = await _todoListService.UpdateTask(workspaceId.Value, taskId, (targetTask) => {
            targetTask.Deadline = newDeadline;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).UpdateTaskDeadline(taskId, updatedTask.Deadline);
    }

    public async Task UpdateTaskOrder(int taskId, int destinationOrder)
    {
        int? workspaceId = await GetConnectedWorkspace();
        if (workspaceId == null) return;

        TodoTask? updatedTask = await _todoListService.UpdateTask(workspaceId.Value, taskId, (targetTask) => {
            targetTask.Order = destinationOrder;
        });
        if (updatedTask == null)
        {
            await Clients.Caller.Error($"Task with id {taskId} does not exist");
            return;
        }

        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).UpdateTaskOrder(taskId, updatedTask.Order);
    }

    public async Task<int?> GetConnectedWorkspace()
    {
        if (!_connectedUsers.Keys.Contains(Context.ConnectionId))
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return null;
        }
        return _connectedUsers.GetGroupId(Context.ConnectionId);
    }

    public async Task ConnectToWorkspace(int workspaceId)
    {
        if (_connectedUsers.Keys.Contains(Context.ConnectionId))
            await DisconnectFromWorkspaces();

        Workspace? targetWorkspace = _todoListService.GetWorkspaceInfo(workspaceId);
        if (targetWorkspace == null)
        {
            await Clients.Caller.Error($"Workspace with id {workspaceId} does not exist");
            return;
        }

        _connectedUsers.Add(Context.ConnectionId, workspaceId);
        int numberOfUsersInGroup = _connectedUsers.Values.Where(connectedWorkspaceId => connectedWorkspaceId == workspaceId).Count();
        string? workspaceGroupName = _connectedUsers.GetGroupName(Context.ConnectionId);
        if (workspaceGroupName == null)
            return;

        await Groups.AddToGroupAsync(Context.ConnectionId, workspaceGroupName);
        await Clients.Group(workspaceGroupName).UserConnected(numberOfUsersInGroup);
    }

    public async Task DisconnectFromWorkspaces()
    {
        if (!_connectedUsers.Keys.Contains(Context.ConnectionId))
        {
            await Clients.Caller.Error($"You are not connected to any workspace");
            return;
        }

        string workspaceGroupName = _connectedUsers.GetGroupName(Context.ConnectionId);
        int? workspaceGroupId = _connectedUsers.GetGroupId(Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, workspaceGroupName);
        _connectedUsers.Remove(Context.ConnectionId);
        int numberOfUsersInGroup = _connectedUsers.Values.Where(connectedWorkspaceId => connectedWorkspaceId == workspaceGroupId).Count();
        await Clients.Group(workspaceGroupName).UserDisconnected(numberOfUsersInGroup);
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
        string workspaceGroupName = _connectedUsers.GetGroupName(Context.ConnectionId);
        int? workspaceGroupId = _connectedUsers.GetGroupId(Context.ConnectionId);
        _connectedUsers.Remove(Context.ConnectionId);
        int numberOfUsersInGroup = _connectedUsers.Values.Where(connectedWorkspaceId => connectedWorkspaceId == workspaceGroupId).Count();
        Clients.Group(workspaceGroupName).UserConnected(_connectedUsers.Count);
        return base.OnDisconnectedAsync(exception);
    }
}
