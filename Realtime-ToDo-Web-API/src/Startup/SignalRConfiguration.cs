using Realtime_ToDo_Web_API.Hubs;

namespace Realtime_ToDo_Web_API.Startup;

public static class SignalRConfiguration
{
	public static WebApplication RegisterHubs(this WebApplication app)
	{
		app.MapHub<TodoListHub>("/Board");
		app.MapHub<WorkspacesHub>("/WorkspacesHub");
		return app;
	}
}
