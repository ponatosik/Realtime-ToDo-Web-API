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

    public async Task AddTask(TodoTask task)
    {
        task.Id = default;
        task.Order = _todoListContext.Tasks.Count() + 1;
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

    public async Task MoveTask(int taskId, int orderDistance)
    {
        TodoTask? targetTask = _todoListContext.Tasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null)
        {
            await Clients.Caller.SendAsync("Error", $"Task with id {taskId} does not exist");
            return;
        }

        int numberOfRecors = _todoListContext.Tasks.Count();
        int order = targetTask.Order;
        orderDistance = Math.Clamp(orderDistance, 1 - order, numberOfRecors - order);
        int moveDirection = Math.Sign(orderDistance);

        if (orderDistance == 0)
        {
            await Clients.Caller.SendAsync("Error", $"Cannot move task with id {taskId}");
            return;
        }

        int lowerBoundOrder = Math.Min(order + orderDistance, order + moveDirection);
        int upperBoundOrder = Math.Max(order + orderDistance, order + moveDirection);
        await _todoListContext.Tasks.Where((task) => task.Order <= upperBoundOrder && task.Order >= lowerBoundOrder).ForEachAsync(task =>
            task.Order -= moveDirection
        );

        targetTask.Order += orderDistance;
        await _todoListContext.SaveChangesAsync();
        await Clients.All.SendAsync(nameof(MoveTask), taskId, orderDistance);
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
