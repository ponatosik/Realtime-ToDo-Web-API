using Realtime_ToDo_Web_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Realtime_ToDo_Web_API.Data;

public class TodoListContext : DbContext
{
    public TodoListContext(DbContextOptions<TodoListContext> options) : base(options)
    {

    }

    public DbSet<Workspace> Workspaces { get; set; }
}
