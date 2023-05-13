namespace Realtime_ToDo_Web_API.Models;

public class TodoTask
{
    public int Id { get; set; }
    public string Title { get; set; } = "New task";
    public bool Completed { get; set; }
    public DateTime? Deadline { get; set; }
    public int Order { get; set; }
}
