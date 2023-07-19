namespace Realtime_ToDo_Web_API.Startup;

public static class CorsConfiguration
{
	public static WebApplication ConfigureCors(this WebApplication app)
	{
		app.UseCors(policy => policy
		  .AllowAnyMethod()
		  .AllowAnyHeader()
		  .SetIsOriginAllowed(origin => true)
		  .AllowCredentials());

		return app;
	}
}