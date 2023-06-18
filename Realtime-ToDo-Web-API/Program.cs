using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Realtime_ToDo_Web_API.Data;
using Realtime_ToDo_Web_API.Hubs;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(builder.Configuration["VersionString"], new OpenApiInfo
    {
        Version = builder.Configuration["VersionString"],
        Title = "Realtime-ToDo-Web-API",
        Description = "A part of realtime web API documentation for creating collaborative todo list",
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});


// Set up your connection string in appsettings.json or dotnet secrets
// Or use in memory database if you have no external database
builder.Services.AddDbContext<TodoListContext>(options =>
    //options.UseInMemoryDatabase("TodoListInMemoryDatabase")
    options.UseSqlServer(builder.Configuration.GetConnectionString("TodoListDB"))
);

// Add custom services and singletons for dependency injection
builder.Services.AddSingleton<WorkspaceRoomManager>();
builder.Services.AddTransient<TodoListService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"/swagger/{builder.Configuration["VersionString"]}/swagger.json", builder.Configuration["VersionString"]);
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(policy => policy
          .AllowAnyMethod()
          .AllowAnyHeader()
          .SetIsOriginAllowed(origin => true)
          .AllowCredentials());

app.MapHub<TodoListHub>("/Board");
app.MapHub<WorkspacesHub>("/WorkspacesHub");


app.Run();
