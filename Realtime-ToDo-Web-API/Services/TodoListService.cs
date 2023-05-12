using Microsoft.EntityFrameworkCore;
using Realtime_ToDo_Web_API.Data;
using Realtime_ToDo_Web_API.Models;

namespace Realtime_ToDo_Web_API.Services;

public class TodoListService
{
    private readonly TodoListContext _todoListContext;
    public TodoListService(TodoListContext todoListContext)
    {
        _todoListContext = todoListContext;
    }

    public async Task<Workspace> AddWorkspace(string worskspaceName) 
    {
        Workspace workspace = new()
        {
            Name = worskspaceName,
            Tasks = new List<TodoTask>()
        };
        await _todoListContext.Workspaces.AddAsync(workspace);
        await _todoListContext.SaveChangesAsync();
        return workspace;
    }

    public IEnumerable<Workspace> GetWorkspaces()
    {
        return _todoListContext.Workspaces;
    }

    public Workspace? GetWorkspaceInfo(int workspaceId) 
    {
        return _todoListContext.Workspaces.FirstOrDefault(workspace => workspace.Id == workspaceId);
    }

    public IEnumerable<TodoTask>? GetWorkspaceTasks(int workspaceId)
    {
        return _todoListContext.Workspaces.Include(workspace => workspace.Tasks).FirstOrDefault(workspace => workspace.Id == workspaceId)?.Tasks;
    }

    public TodoTask? GetTask(int workspaceId, int taskId)
    {
        return  _todoListContext.Workspaces
                .Include(workspace => workspace.Tasks)
                ?.FirstOrDefault(workspace => workspace.Id == workspaceId)
                ?.Tasks
                ?.FirstOrDefault(task => task.Id == taskId);
    }

    public async Task<TodoTask> AddTask(int workspaceId, TodoTask task) 
    {
        Workspace? workspace = _todoListContext.Workspaces.Include(workspace => workspace.Tasks).FirstOrDefault(workspace => workspace.Id == workspaceId);
        if (workspace == null)
            return null;

        task.Id = default;
        task.Order = workspace.Tasks.Count();

        workspace.Tasks.Add(task);
        await _todoListContext.SaveChangesAsync();
        return task;
    }

    public async Task<TodoTask?> UpdateTask(int workspaceId, int taskId, Action<TodoTask> modifierDelegate) 
    {
        TodoTask? targetTask = GetTask(workspaceId, taskId);
        if (targetTask == null) return null;

        int id = targetTask.Id;
        int order = targetTask.Order;

        modifierDelegate(targetTask);

        targetTask.Id = id;
        if (targetTask.Order != order) 
        {
            var workspaceTasks = GetWorkspaceTasks(workspaceId);

            targetTask.Order = Math.Clamp(targetTask.Order, 0, workspaceTasks.Count() - 1);
            int lowerBoundOrder = order < targetTask.Order ? order : targetTask.Order;
            int upperBoundOrder = order > targetTask.Order ? order : targetTask.Order;
            int shiftDirection = Math.Sign(targetTask.Order - order);

            foreach (TodoTask task in workspaceTasks.Where((task) => task.Order > targetTask.Order))
                task.Order -= shiftDirection;
        }

        await _todoListContext.SaveChangesAsync();
        return targetTask;
    }

    public async Task<TodoTask?> DeleteTask(int workspaceId, int taskId) 
    {
        var workspaceTasks = _todoListContext.Workspaces.Include(workspace => workspace.Tasks).FirstOrDefault(workspace => workspace.Id == workspaceId)?.Tasks;
        if (workspaceTasks == null) return null;

        TodoTask? targetTask = workspaceTasks.FirstOrDefault(task => task.Id == taskId);
        if (workspaceTasks == null) return null;

        foreach (TodoTask task in workspaceTasks.Where((task) => task.Order > targetTask.Order))
            task.Order--;

        workspaceTasks.Remove(targetTask);
        await _todoListContext.SaveChangesAsync();
        return targetTask;
    }
}
