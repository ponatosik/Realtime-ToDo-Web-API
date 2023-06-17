using Realtime_ToDo_Web_API.Models;

namespace Realtime_ToDo_Web_API.Hubs;

/// <summary>
/// Represents the client interface for <see cref="TodoListHub">TodoList SignalR hub</see>.
/// Defines about which actions in todo list the client can be notified about
/// </summary>
public interface ITodoListClient
{
    /// <summary>
    /// Notifies the client that a new task has been added to the todo list.
    /// </summary>
    /// <param name="task">The task that was added.</param>
    Task AddTask(TodoTask task);
    /// <summary>
    /// Notifies the client that an existing task in the todo list has been updated.
    /// </summary>
    /// <param name="newTask">The updated task.</param>
    Task UpdateTask(TodoTask newTask);
    /// <summary>
    /// Notifies the client that the title of a task in the todo list has been updated.
    /// </summary>
    /// <param name="taskId">The identifier of the task.</param>
    /// <param name="title">The new title of the task.</param>
    Task UpdateTaskTitle(int taskId, string title);
    /// <summary>
    /// Notifies the client that the completion status of a task in the todo list has been updated.
    /// </summary>
    /// <param name="taskId">The identifier of the task.</param>
    /// <param name="completed">The new completion status of the task.</param>
    Task UpdateTaskCompleted(int taskId, bool completed);
    /// <summary>
    /// Notifies the client that the deadline of a task in the todo list has been updated.
    /// </summary>
    /// <param name="taskId">The identifier of the task.</param>
    /// <param name="deadline">The new deadline of the task.</param>
    Task UpdateTaskDeadline(int taskId, DateTime? deadline);
    /// <summary>
    /// Notifies the client that the order of a task in the todo list has been updated.
    /// </summary>
    /// <param name="taskId">The identifier of the task.</param>
    /// <param name="order">The new order of the task.</param>
    Task UpdateTaskOrder(int taskId, int order);
    /// <summary>
    /// Notifies the client that a task has been deleted from the todo list.
    /// </summary>
    /// <param name="taskId">The identifier of the task that was deleted.</param>
    Task DeleteTask(int taskId);

    /// <summary>
    /// Notifies the client that they have successfully connected to a workspace.
    /// </summary>
    /// <param name="workspaceId">The identifier of the connected workspace.</param>
    Task Connected(int workspaceId);
    /// <summary>
    /// Notifies the client that they have been disconnected from a workspace.
    /// </summary>
    /// <param name="workspaceId">The identifier of the disconnected workspace.</param>
    Task Disconnected(int workspaceId);

    /// <summary>
    /// Notifies the client that new user have connected to a workspace.
    /// </summary>
    /// <param name="userCount">Current number of connected users.</param>
    Task UserConnected(int userCount);
    /// <summary>
    /// Notifies the client that a user have dissconnected from a workspace.
    /// </summary>
    /// <param name="userCount">Current number of connected users.</param>
    Task UserDisconnected(int userCount);

    /// <summary>
    /// Notifies the client about an error that occurred.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    Task Error(string errorMessage);
}
