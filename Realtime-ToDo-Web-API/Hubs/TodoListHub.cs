using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Realtime_ToDo_Web_API.Data;
using Realtime_ToDo_Web_API.Models;
using System.Threading.Tasks;

namespace Realtime_ToDo_Web_API.Hubs;

public class TodoListHub : Hub
{
    public readonly IUserConnectionStorage _connectedUsers;
    private readonly TodoListContext _todoListContext;
    public TodoListHub(TodoListContext todoListContext, IUserConnectionStorage connectedUsers)
    {
        _todoListContext = todoListContext;
        _connectedUsers = connectedUsers;
    }

    public async Task AddTask(string title, DateTime? deadline)
    {
        Workspace? workspace = await _getConnectedWorkspaceAsync();
        if (workspace == null)
            return;

        TodoTask task = new TodoTask
        {
            Title = title,
            Deadline = deadline,
            Order = workspace.Tasks.Count()
        };

        workspace.Tasks.Add(task);
        await _todoListContext.SaveChangesAsync();
        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).SendAsync(nameof(AddTask), task);
    }

    public async Task UpdateTask(int taskId, TodoTask newTask)
    {
        Workspace? workspace = await _getConnectedWorkspaceAsync();
        if (workspace == null)
            return;

        TodoTask? targetTask = workspace.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        targetTask.Title = newTask.Title;
        targetTask.Completed = newTask.Completed;
        targetTask.Deadline = newTask.Deadline;

        if (targetTask.Order != newTask.Order)
            await MoveTask(taskId, newTask.Order);

        await _todoListContext.SaveChangesAsync();
        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).SendAsync(nameof(UpdateTask), targetTask);
    }

    public async Task DeleteTask(int taskId)
    {
        Workspace? workspace = await _getConnectedWorkspaceAsync();
        if (workspace == null)
            return;

        TodoTask? targetTask = workspace.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        foreach (TodoTask task in workspace.Tasks.Where((task) => task.Order > targetTask.Order))
            task.Order--;

        workspace.Tasks.Remove(targetTask);
        await _todoListContext.SaveChangesAsync();
        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).SendAsync(nameof(DeleteTask), taskId);
    }

    public async Task UpdateTaskTitle(int taskId, string newTitle)
    {
        Workspace? workspace = await _getConnectedWorkspaceAsync();
        if (workspace == null)
            return;

        TodoTask? targetTask = workspace.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        targetTask.Title = newTitle;
        await _todoListContext.SaveChangesAsync();
        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).SendAsync(nameof(UpdateTaskTitle), taskId, newTitle);
    }

    public async Task UpdateTaskCompleted(int taskId, bool newCompleted)
    {
        Workspace? workspace = await _getConnectedWorkspaceAsync();
        if (workspace == null)
            return;

        TodoTask? targetTask = workspace.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        targetTask.Completed = newCompleted;
        await _todoListContext.SaveChangesAsync();
        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).SendAsync(nameof(UpdateTaskCompleted), taskId, newCompleted);
    }

    public async Task UpdateTaskDeadline(int taskId, DateTime newDeadline)
    {
        Workspace? workspace = await _getConnectedWorkspaceAsync();
        if (workspace == null)
            return;

        TodoTask? targetTask = workspace.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        targetTask.Deadline = newDeadline;
        await _todoListContext.SaveChangesAsync();
        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).SendAsync(nameof(UpdateTaskDeadline), taskId, newDeadline);
    }

    public async Task MoveTask(int taskId, int destinationOrder)
    {
        Workspace? workspace = await _getConnectedWorkspaceAsync();
        if (workspace == null)
            return;

        TodoTask? targetTask = workspace.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        int order = targetTask.Order;
        destinationOrder = Math.Clamp(destinationOrder, 0, workspace.Tasks.Count() - 1);

        if (order == destinationOrder)
            return;

        int lowerBoundOrder = order < destinationOrder ? order : destinationOrder;
        int upperBoundOrder = order > destinationOrder ? order : destinationOrder;
        int shiftDirection = Math.Sign(destinationOrder - order);

        foreach (TodoTask task in workspace.Tasks.Where((task) => task.Order > targetTask.Order))
            task.Order -= shiftDirection;
        
        targetTask.Order = destinationOrder;
        await _todoListContext.SaveChangesAsync();
        await Clients.Group(_connectedUsers.GetGroupName(Context.ConnectionId)).SendAsync(nameof(MoveTask), taskId, destinationOrder);
    }

    public async Task ConnectToWorkspace(int workspaceId)
    {
        if (_connectedUsers.Keys.Contains(Context.ConnectionId))
            await DisconnectFromoWorkspaces();

        Workspace? targetWorkspace = _todoListContext.Workspaces.FirstOrDefault(workspace => workspace.Id == workspaceId);
        if (targetWorkspace == null)
        {
            await Clients.Caller.SendAsync("Error", $"Workspace with id {workspaceId} does not exist");
            return;
        }

        _connectedUsers.Add(Context.ConnectionId, workspaceId);
        int numberOfUsersInGroup = _connectedUsers.Values.Where(connectedWorkspaceId => connectedWorkspaceId == workspaceId).Count();
        string? workspaceGroupName = _connectedUsers.GetGroupName(Context.ConnectionId);
        if (workspaceGroupName == null)
            return;

        await Groups.AddToGroupAsync(Context.ConnectionId, workspaceGroupName);
        await Clients.Group(workspaceGroupName).SendAsync("UserConnected", numberOfUsersInGroup);
    }

    public async Task DisconnectFromoWorkspaces() 
    {
        if (!_connectedUsers.Keys.Contains(Context.ConnectionId)) 
        {
            await Clients.Caller.SendAsync("Error", $"You are not connected to any workspace");
            return;
        }

        string workspaceGroupName = _connectedUsers.GetGroupName(Context.ConnectionId);
        int? workspaceGroupId = _connectedUsers.GetGroupId(Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, workspaceGroupName);
        _connectedUsers.Remove(Context.ConnectionId);
        int numberOfUsersInGroup = _connectedUsers.Values.Where(connectedWorkspaceId => connectedWorkspaceId == workspaceGroupId).Count();
        await Clients.Group(workspaceGroupName).SendAsync("UserDisconnected", numberOfUsersInGroup);
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext != null && httpContext.Request.Query.Keys.Contains("workspaceid"))
        {
            string workspaceIdString = httpContext.Request.Query["workspaceid"];
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
        Clients.Group(workspaceGroupName).SendAsync("UserDisconnected", _connectedUsers.Count);
        return base.OnDisconnectedAsync(exception);
    }

    private async Task<Workspace?> _getConnectedWorkspaceAsync() 
    {
        if (!_connectedUsers.Keys.Contains(Context.ConnectionId))
        {
            await Clients.Caller.SendAsync("Error", $"You are not connected to any workspace");
            return null;
        }

        int? connectedWorkspaceId = _connectedUsers.GetGroupId(Context.ConnectionId);
        Workspace? workspace = _todoListContext.Workspaces.Include(workspace => workspace.Tasks).FirstOrDefault(workspace => workspace.Id == connectedWorkspaceId);
        if (workspace == null)
        {
            await Clients.Caller.SendAsync("Error", $"Workspace with id {connectedWorkspaceId} does not exist");
            return null;
        }
        return workspace;
    }
}
