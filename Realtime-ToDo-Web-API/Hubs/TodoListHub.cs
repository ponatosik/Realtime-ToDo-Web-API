using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Realtime_ToDo_Web_API.Data;
using Realtime_ToDo_Web_API.Models;

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
        TodoTask task = new TodoTask 
        {
            Title = title,
            Deadline = deadline,
            Order = _todoListContext.Tasks.Count()
        }; 
        await _todoListContext.Tasks.AddAsync(task);
        await _todoListContext.SaveChangesAsync();
        await Clients.All.SendAsync(nameof(AddTask), task);
    }

    public async Task UpdateTask(int taskId, TodoTask newTask)
    {
        TodoTask? targetTask = _todoListContext.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null) 
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        targetTask.Title = newTask.Title;
        targetTask.Completed = newTask.Completed;
        targetTask.Deadline = newTask.Deadline;
        targetTask.Order = newTask.Order;

        await _todoListContext.SaveChangesAsync();
        await Clients.All.SendAsync(nameof(UpdateTask), targetTask);
    }

    public async Task DeleteTask(int taskId)
    {
        TodoTask? targetTask = _todoListContext.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        await _todoListContext.Tasks.Where((task) => task.Order > targetTask.Order).ForEachAsync(task =>
            task.Order--
        );

        _todoListContext.Tasks.Remove(targetTask);
        await _todoListContext.SaveChangesAsync();
        await Clients.All.SendAsync(nameof(DeleteTask), taskId);
    }

    public async Task UpdateTaskTitle(int taskId, string newTitle)
    {
        TodoTask? targetTask = _todoListContext.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        targetTask.Title = newTitle;
        await _todoListContext.SaveChangesAsync();
        await Clients.All.SendAsync(nameof(UpdateTaskTitle), taskId, newTitle);
    }

    public async Task UpdateTaskCompleted(int taskId, bool newCompleted)
    {
        TodoTask? targetTask = _todoListContext.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        targetTask.Completed = newCompleted;
        await _todoListContext.SaveChangesAsync();
        await Clients.All.SendAsync(nameof(UpdateTaskCompleted), taskId, newCompleted);
    }

    public async Task UpdateTaskDeadline(int taskId, DateTime newDeadline)
    {
        TodoTask? targetTask = _todoListContext.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        targetTask.Deadline = newDeadline;
        await _todoListContext.SaveChangesAsync();
        await Clients.All.SendAsync(nameof(UpdateTaskDeadline), taskId, newDeadline);
    }

    public async Task MoveTask(int taskId, int destinationOrder)
    {
        TodoTask? targetTask = _todoListContext.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        int order = targetTask.Order;
        destinationOrder = Math.Clamp(destinationOrder, 0, _todoListContext.Tasks.Count() - 1);

        if (order == destinationOrder)
        {
            await Clients.Caller.SendAsync("Error", $"Cannot move task with id {taskId}");
            return;
        }

        int lowerBoundOrder = order < destinationOrder ? order : destinationOrder;
        int upperBoundOrder = order > destinationOrder ? order : destinationOrder;
        int shiftDirection = Math.Sign(destinationOrder - order);

        await _todoListContext.Tasks.Where((task) => task.Order <= upperBoundOrder && task.Order >= lowerBoundOrder).ForEachAsync(task =>
            task.Order -= shiftDirection
        );
        targetTask.Order = destinationOrder;
        await _todoListContext.SaveChangesAsync();
        await Clients.All.SendAsync(nameof(MoveTask), taskId, destinationOrder);
    }

    public override Task OnConnectedAsync()
    {
        _connectedUsers.Add(Context.ConnectionId);
        Clients.All.SendAsync("UserConnected", _connectedUsers.Count);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _connectedUsers.Remove(Context.ConnectionId);
        Clients.All.SendAsync("UserDisconnected", _connectedUsers.Count);
        return base.OnDisconnectedAsync(exception);
    }
}
