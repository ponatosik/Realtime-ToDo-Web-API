using Realtime_ToDo_Web_API.Services.SignalR;
using Realtime_ToDo_Web_API.Services;

namespace Realtime_ToDo_Web_API.Startup;

public static class DependencyInjectionSetup
{
	public static IServiceCollection RegisterServices(this IServiceCollection services) 
	{
		services.AddSingleton<WorkspaceRoomManager>();
		services.AddTransient<TodoListService>();
		return services;
	}
}
