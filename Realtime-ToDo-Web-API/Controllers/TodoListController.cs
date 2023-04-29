using Microsoft.AspNetCore.Mvc;
using Realtime_ToDo_Web_API.Models;
using Realtime_ToDo_Web_API.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Realtime_ToDo_Web_API.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoListController : ControllerBase
{
    private readonly TodoListContext _todoListContext;
    public TodoListController(TodoListContext todoListContext)
    {
        _todoListContext = todoListContext;
    }

    [HttpGet(Name = "GetTodoList")]
    public IEnumerable<TodoTask> Get()
    {
        return _todoListContext.Tasks.OrderBy(task => task.Order);
    }

    [HttpPut(Name = "PutTodoList")]
    public async Task<TodoTask> PutAsync(TodoTask task)
    {
        task.Id = default;
        task.Order = _todoListContext.Tasks.Count() + 1;
        await _todoListContext.Tasks.AddAsync(task);
        await _todoListContext.SaveChangesAsync();
        return task;
    }
}