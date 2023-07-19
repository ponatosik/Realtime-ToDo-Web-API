using Microsoft.EntityFrameworkCore;
using Realtime_ToDo_Web_API.Data;
using Realtime_ToDo_Web_API.Services;

namespace Realtime_ToDo_Web_API.Tests;

public class TodoListServiceTests : IDisposable
{
    private TodoListContext _dbContext;
    private TodoListService _todoListService;

    public TodoListServiceTests()
    {
        var dbContextOptions = new DbContextOptionsBuilder<TodoListContext>()
            .UseInMemoryDatabase($"TEST_DATABASE_{this.GetHashCode()}")
            .Options;

        _dbContext = new TodoListContext(dbContextOptions);
        _dbContext.Database.EnsureCreated();
        _todoListService = new TodoListService(_dbContext);
    }

    [Fact]
    public void AddWorkspace_SingleWorkspace_ReturnsWorkspaceInfoWithProvidedName()
    {
        var expectedName = "Test workspace";

        var actual = _todoListService.AddWorkspace(expectedName).Result.Name;

        Assert.Equal(expectedName, actual);
    }

    [Fact]
    public void AddWorkspace_SingleWorkspace_ReturnsWorkspaceInfoWithNoTasks()
    {
        var workspaceName = "Test workspace";

        var actual = _todoListService.AddWorkspace(workspaceName).Result.TaskCount;

        Assert.Equal(0, actual);
    }

    [Fact]
    public void AddWorkspace_SingleWorkspace_CreatesWorkspaceInDatabase()
    {
        var workspaceName = "Test workspace";

        var actual = _todoListService.AddWorkspace(workspaceName).Result.Id;

        Assert.NotEmpty(_dbContext.Workspaces);
        Assert.Contains(actual, _dbContext.Workspaces.Select(x => x.Id));
    }

    [Fact]
    public void UpdateWorkspaceInfo_SingleWorkspace_ReturnsUpdatedWorkspaceName()
    {
        var newName = "Updated workspace";
        var createdWorkspace = _todoListService.AddWorkspace("Test workspace").Result;

        var actual = _todoListService.UpdateWorkspaceInfo(createdWorkspace.Id, (x) => x.Name = newName).Result!.Name;

        Assert.Equal(newName, actual);
    }

    [Fact]
    public void UpdateWorkspaceInfo_SingleWorkspace_UpdatesWorkspaceInDatabase()
    {
        var newName = "Updated workspace";
        var createdWorkspace = _todoListService.AddWorkspace("Test workspace").Result;
        var workspaceId = createdWorkspace.Id;

        var actual = _todoListService.UpdateWorkspaceInfo(workspaceId, (x) => x.Name = newName).Result!.Name;

        Assert.NotEmpty(_dbContext.Workspaces);
        Assert.Contains(newName, _dbContext.Workspaces.Select(x => x.Name));
    }


    [Fact]
    public void GetWorkspaceInfo_NotEmptyWorkspace_ReturnsValidTaskCount()
    {
        var createdWorkspace = _todoListService.AddWorkspace("Test workspace").Result;
        var workspaceId = createdWorkspace.Id;
        _todoListService.AddTask(workspaceId, new()).Wait();
        _todoListService.AddTask(workspaceId, new()).Wait();
        _todoListService.AddTask(workspaceId, new()).Wait();

        int actual = _todoListService.GetWorkspaceInfo(workspaceId)!.TaskCount;

        Assert.Equal(3, actual);
    }

    [Fact]
    public void DeleteWorkspace_SingleWorkspace_ReturnsDeletedWorkspaceInfo()
    {
        var workspaceName = "Test workspace";
        var createdWorkspace = _todoListService.AddWorkspace(workspaceName).Result;
        var workspaceId = createdWorkspace.Id;

        var actual = _todoListService.DeleteWorkspace(workspaceId)!.Result;

        Assert.Equal(workspaceName, actual!.Name);
        Assert.Equal(workspaceId, actual!.Id);

    }

    [Fact]
    public void DeleteWorkspace_SingleWorkspace_DeletesWorkspaceInDatabase()
    {
        var workspaceName = "Test workspace";
        var createdWorkspace = _todoListService.AddWorkspace(workspaceName).Result;
        var workspaceId = createdWorkspace.Id;

        _todoListService.DeleteWorkspace(workspaceId).Wait();

        Assert.DoesNotContain(workspaceName, _dbContext.Workspaces.Select(x => x.Name));
        Assert.DoesNotContain(workspaceId, _dbContext.Workspaces.Select(x => x.Id));
    }

    [Fact]
    public void AddTask_SingleTask_ReturnsCreatedTask()
    {
        var createdWorkspace = _todoListService.AddWorkspace("Test workspace").Result;
        var workspaceId = createdWorkspace.Id;
        var expectedTaskTitle = "New Task";
        var expectedTask = new Models.TodoTask() { Title = expectedTaskTitle };

        var actual = _todoListService.AddTask(workspaceId, expectedTask).Result!.Title;

        Assert.Equal(expectedTaskTitle, actual);
    }

    [Fact]
    public void AddTask_SingleTask_CreatesTaskInDatabase()
    {
        var createdWorkspace = _todoListService.AddWorkspace("Test workspace").Result;
        var workspaceId = createdWorkspace.Id;
        var expectedTaskTitle = "New Task";
        var expectedTask = new Models.TodoTask() { Title = expectedTaskTitle };

        var actual = _todoListService.AddTask(workspaceId, expectedTask).Result!.Title;

        Assert.NotEmpty(_dbContext.Workspaces.Include(x => x.Tasks).First(x => x.Id == workspaceId).Tasks!);
        Assert.Contains(expectedTaskTitle, _dbContext.Workspaces.Include(x => x.Tasks)
                                                                .First(x => x.Id == workspaceId).Tasks!
                                                                .Select(x => x.Title));
    }

    [Fact]
    public void AddTask_ManyTasks_ReturnsCreatedTasksInProvidedOrder()
    {
        var createdWorkspace = _todoListService.AddWorkspace("Test workspace").Result;
        var workspaceId = createdWorkspace.Id;
        var task0 = new Models.TodoTask() { Title = "Task0" };
        var task1 = new Models.TodoTask() { Title = "Task1" };
        var task2 = new Models.TodoTask() { Title = "Task2" };

        var actual0 = _todoListService.AddTask(workspaceId, task0).Result!.Order;
        var actual1 = _todoListService.AddTask(workspaceId, task1).Result!.Order;
        var actual2 = _todoListService.AddTask(workspaceId, task2).Result!.Order;

        Assert.Equal(0, actual0);
        Assert.Equal(1, actual1);
        Assert.Equal(2, actual2);
    }

    [Fact]
    public void UpdateTask_SingleTask_ReturnsUpdatedTask()
    {
        var taskTitle = $"Updated task";
        var taskCompleted = true;
        var taskDeadline = System.DateTime.Now;
        var createdWorkspace = _todoListService.AddWorkspace("Test workspace").Result;
        var workspaceId = createdWorkspace.Id;
        var createdTask = _todoListService.AddTask(workspaceId, new());

        var actual = _todoListService.UpdateTask(workspaceId, createdTask.Id, (task) =>
        {
            task.Title = taskTitle;
            task.Completed = taskCompleted;
            task.Deadline = taskDeadline;
        }).Result!;

        Assert.NotNull(actual);
        Assert.Equal(taskTitle, actual.Title);
        Assert.Equal(taskCompleted, actual.Completed);
        Assert.Equal(taskDeadline, actual.Deadline);
    }

    [Fact]
    public void UpdateTask_SingleTask_UpdatesTask()
    {
        var taskTitle = "Updated task";
        var taskCompleted = true;
        var taskDeadline = System.DateTime.Now;
        var workspaceId = _todoListService.AddWorkspace("Test workspace").Result.Id;
        var createdTask = _todoListService.AddTask(workspaceId, new()).Result!;
        var taskId = createdTask.Id;

        _todoListService.UpdateTask(workspaceId, createdTask.Id, (task) =>
        {
            task.Title = taskTitle;
            task.Completed = taskCompleted;
            task.Deadline = taskDeadline;
        }).Wait();

        Assert.True(_dbContext.Workspaces.Include(x => x.Tasks)
                                         .First(x => x.Id == workspaceId).Tasks!
                                         .Exists(x => x.Title == taskTitle &&
                                                      x.Completed == taskCompleted &&
                                                      x.Deadline == taskDeadline));

    }

    [Fact]
    public void UpdateTask_WithinWorkspace_ChangesTasksOrderInDatabase()
    {
        var createdWorkspace = _todoListService.AddWorkspace("Test workspace").Result;
        var workspaceId = createdWorkspace.Id;
        var task0 = _todoListService.AddTask(workspaceId, new() { Title = "Task0" }).Result!.Id;
        var task1 = _todoListService.AddTask(workspaceId, new() { Title = "Task1" }).Result!.Id;
        var task2 = _todoListService.AddTask(workspaceId, new() { Title = "Task2" }).Result!.Id;
        var task3 = _todoListService.AddTask(workspaceId, new() { Title = "Task3" }).Result!.Id;

        _todoListService.UpdateTask(workspaceId, task0, (task) =>
        {
            task.Order = 2;
        }).Wait();

        Assert.Equal(2, _dbContext.Workspaces.Include(x => x.Tasks)
                                             .First(x => x.Id == workspaceId).Tasks!
                                             .First(x => x.Id == task0).Order);
        Assert.Equal(0, _dbContext.Workspaces.Include(x => x.Tasks)
                                             .First(x => x.Id == workspaceId).Tasks!
                                             .First(x => x.Id == task1).Order);
        Assert.Equal(1, _dbContext.Workspaces.Include(x => x.Tasks)
                                             .First(x => x.Id == workspaceId).Tasks!
                                             .First(x => x.Id == task2).Order);
        Assert.Equal(3, _dbContext.Workspaces.Include(x => x.Tasks)
                                             .First(x => x.Id == workspaceId).Tasks!
                                             .First(x => x.Id == task3).Order);
    }

    [Fact]
    public void DeleteTask_SingleTask_ReturnsDeletedTask()
    {
        var taskTitle = "Task to delete";
        var workspaceId = _todoListService.AddWorkspace("Test workspace").Result!.Id;
        var createdTask = _todoListService.AddTask(workspaceId, new() { Title = taskTitle }).Result!.Id;

        var actual = _todoListService.DeleteTask(workspaceId, createdTask).Result!;

        Assert.Equal(taskTitle, actual.Title);
    }

    [Fact]
    public void DeleteTask_SingleTask_DeletesTaskInDatabase()
    {
        var workspaceId = _todoListService.AddWorkspace("Test workspace").Result.Id;
        var createdTask = _todoListService.AddTask(workspaceId, new()).Id;

        _todoListService.DeleteTask(workspaceId, createdTask).Wait();

        Assert.DoesNotContain(createdTask, _dbContext.Workspaces.Include(x => x.Tasks)
                                                                .First(x => x.Id == workspaceId).Tasks!
                                                                .Select(x => x.Id));
    }

    [Fact]
    public void DeleteTask_WithOtherTasks_ChangesTasksOrder()
    {
        var createdWorkspace = _todoListService.AddWorkspace("Test workspace").Result;
        var workspaceId = createdWorkspace.Id;
        var task0 = _todoListService.AddTask(workspaceId, new() { Title = "Task0" }).Result!.Id;
        var task1 = _todoListService.AddTask(workspaceId, new() { Title = "Task1" }).Result!.Id;
        var task2 = _todoListService.AddTask(workspaceId, new() { Title = "Task2" }).Result!.Id;
        var task3 = _todoListService.AddTask(workspaceId, new() { Title = "Task3" }).Result!.Id;

        _todoListService.DeleteTask(workspaceId, task1).Wait();

        Assert.Equal(0, _dbContext.Workspaces.Include(x => x.Tasks)
                                             .First(x => x.Id == workspaceId).Tasks!
                                             .First(x => x.Id == task0).Order);
        Assert.Equal(1, _dbContext.Workspaces.Include(x => x.Tasks)
                                             .First(x => x.Id == workspaceId).Tasks!
                                             .First(x => x.Id == task2).Order);
        Assert.Equal(2, _dbContext.Workspaces.Include(x => x.Tasks)
                                             .First(x => x.Id == workspaceId).Tasks!
                                             .First(x => x.Id == task3).Order);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}