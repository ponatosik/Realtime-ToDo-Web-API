namespace Realtime_ToDo_Web_API.Models;

/// <summary>
/// Represents a task in a to-do list.
/// </summary>
public class TodoTask
{
    /// <summary>
    /// The unique identifier of the task.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The title of the task.
    /// </summary>
    public string Title { get; set; } = "New task";

    /// <summary>
    /// A value indicating whether the task is completed.
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// The deadline for the task.
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// The order of the task in the list.
    /// </summary>
    public int Order { get; set; }
}
