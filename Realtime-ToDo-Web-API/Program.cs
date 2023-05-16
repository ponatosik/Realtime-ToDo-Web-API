using Microsoft.EntityFrameworkCore;
using Realtime_ToDo_Web_API.Data;
using Realtime_ToDo_Web_API.Hubs;
using Realtime_ToDo_Web_API.Services;
using Realtime_ToDo_Web_API.Services.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Set up your connection string in appsettings.json or dotnet secrets
// Or use in memory database if you have no external database
builder.Services.AddDbContext<TodoListContext>(options =>
    //options.UseInMemoryDatabase("TodoListInMemoryDatabase")
    options.UseSqlServer(builder.Configuration.GetConnectionString("TodoListDB"))
);

// Add custom services and singletons for dependency injection
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddTransient<TodoListService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(policy => policy
          .AllowAnyMethod()
          .AllowAnyHeader()
          .SetIsOriginAllowed(origin => true)
          .AllowCredentials());

app.MapHub<TodoListHub>("/Board");

app.Run();
