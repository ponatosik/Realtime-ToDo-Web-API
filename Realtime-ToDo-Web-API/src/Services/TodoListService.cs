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

    public async Task<WorkspaceInfo> AddWorkspace(string worskspaceName) 
    {
        Workspace workspace = new()
        {
            Name = worskspaceName,
            Tasks = new List<TodoTask>()
        };
        await _todoListContext.Workspaces.AddAsync(workspace);
        await _todoListContext.SaveChangesAsync();
        return new WorkspaceInfo(workspace);
    }
    public async Task<WorkspaceInfo?> UpdateWorkspaceInfo(int workspaceId, Action<WorkspaceInfo> modifierDelegate)
    {
        WorkspaceInfo? targetWorkspaceInfo = GetWorkspaceInfo(workspaceId);
        if (targetWorkspaceInfo == null) return null;

        modifierDelegate(targetWorkspaceInfo);
        targetWorkspaceInfo.Id = workspaceId;

        Workspace? targetWorkspace = GetWorkspace(workspaceId);
        if (targetWorkspace == null) return null;

        targetWorkspace.Name = targetWorkspaceInfo.Name;

        await _todoListContext.SaveChangesAsync();
        return targetWorkspaceInfo;
    }
    public async Task<WorkspaceInfo?> DeleteWorkspace(int workspaceId)
    {
        Workspace? targetWorkspace = GetWorkspace(workspaceId);
        if (targetWorkspace == null) return null;

        _todoListContext.Workspaces?.Remove(targetWorkspace);
        await _todoListContext.SaveChangesAsync();
        return new WorkspaceInfo(targetWorkspace);
    }
    public IEnumerable<WorkspaceInfo> GetWorkspacesInfo()
    {
        return _todoListContext.Workspaces
            .Include(workspace => workspace.Tasks)
            .Select(workspace => new WorkspaceInfo(workspace, workspace.Tasks!.Count));
    }
    public WorkspaceInfo? GetWorkspaceInfo(int workspaceId) 
    {
        Workspace? targetWorkspace = GetWorkspace(workspaceId);

        if (targetWorkspace == null)
            return null;

        return new WorkspaceInfo(targetWorkspace);
    }
    public Workspace? GetWorkspace(int workspaceId)
    {
        return  _todoListContext.Workspaces
                .FirstOrDefault(workspace => workspace.Id == workspaceId);
    }
    public Workspace? GetWorkspaceWithTasks(int workspaceId)
    {
        return  _todoListContext.Workspaces
                .Include(workspace => workspace.Tasks)
                .FirstOrDefault(workspace => workspace.Id == workspaceId);
    }
    public IList<TodoTask>? GetWorkspaceTasks(int workspaceId)
    {
        return  _todoListContext.Workspaces
                .Include(workspace => workspace.Tasks)
                .FirstOrDefault(workspace => workspace.Id == workspaceId)
                ?.Tasks;
    }
    public int? GetWorkspaceTasksCount(int workspaceId)
    {
        return _todoListContext.Workspaces
                .Include(workspace => workspace.Tasks)
                .FirstOrDefault(workspace => workspace.Id == workspaceId)
                ?.Tasks
                ?.Count;
    }
    public TodoTask? GetTask(int workspaceId, int taskId)
    {
        return  _todoListContext.Workspaces
                .Include(workspace => workspace.Tasks)
                ?.FirstOrDefault(workspace => workspace.Id == workspaceId)
                ?.Tasks
                ?.Find(task => task.Id == taskId);
    }
    public async Task<TodoTask?> AddTask(int workspaceId, TodoTask task) 
    {
        Workspace? workspace = GetWorkspaceWithTasks(workspaceId);
        if (workspace == null)
            return null;

        task.Id = default;
        task.Order = workspace.Tasks!.Count;

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
            if (workspaceTasks == null) return null;

            int newOrded = Math.Clamp(targetTask.Order, 0, workspaceTasks.Count() - 1);
            int lowerBoundOrder = order < newOrded ? order : newOrded;
            int upperBoundOrder = order > newOrded ? order : newOrded;
            int shiftDirection = Math.Sign(newOrded - order);

            foreach (TodoTask task in workspaceTasks.Where((task) => task.Order <= upperBoundOrder && task.Order >= lowerBoundOrder))
                task.Order -= shiftDirection;

            targetTask.Order = newOrded;
        }

        await _todoListContext.SaveChangesAsync();
        return targetTask;
    }
    public async Task<TodoTask?> DeleteTask(int workspaceId, int taskId) 
    {
        var workspaceTasks = GetWorkspaceTasks(workspaceId);
        if (workspaceTasks == null) return null;

        TodoTask? targetTask = workspaceTasks.FirstOrDefault(task => task.Id == taskId);
        if (targetTask == null) return null;

        foreach (TodoTask task in workspaceTasks.Where((task) => task.Order > targetTask.Order))
            task.Order--;

        workspaceTasks.Remove(targetTask);
        await _todoListContext.SaveChangesAsync();
        return targetTask;
    }
}
