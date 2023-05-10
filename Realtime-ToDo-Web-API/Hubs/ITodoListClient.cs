using Realtime_ToDo_Web_API.Models;

namespace Realtime_ToDo_Web_API.Hubs;

public interface ITodoListClient
{
    Task AddTask(TodoTask task);
    Task UpdateTask(TodoTask newTask);
    Task UpdateTaskTitle(int taskId, string title);
    Task UpdateTaskCompleted(int taskId, bool completed);
    Task UpdateTaskDeadline(int taskId, DateTime? deadline);
    Task UpdateTaskOrder(int taskId, int order);
    Task DeleteTask(int taskId);

    Task UserConnected(int userCount);
    Task UserDisconnected(int userCount);

    Task Error(string errorMessage);
}
