using Realtime_ToDo_Web_API.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

string ApiVersion = builder.Configuration["VersionString"]!;
builder.Services.RegisterSwagger(ApiVersion);

builder.ConfigureDatabase();

builder.Services.RegisterServices();

var app = builder.Build();

app.ConfigureSwagger(ApiVersion);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.ConfigureCors();

app.RegisterHubs();


app.Run();
