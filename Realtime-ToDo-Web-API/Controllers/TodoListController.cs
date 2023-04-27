using Microsoft.AspNetCore.Mvc;
using Realtime_ToDo_Web_API.Models;

namespace Realtime_ToDo_Web_API.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoListController : ControllerBase
{
    public TodoListController()
    {

    }

    [HttpGet(Name = "GetTodoList")]
    public IEnumerable<TodoTask> Get()
    {
        return new List<TodoTask> {
            new TodoTask{Title = "Create new repo", Completed = true, Id = 0, Order = 0},
            new TodoTask{Title = "Add basic controller", Completed = true, Id = 1, Order = 1},
            new TodoTask{Title = "Set up database", Completed = false, Id = 2, Order = 2}};
    }
}