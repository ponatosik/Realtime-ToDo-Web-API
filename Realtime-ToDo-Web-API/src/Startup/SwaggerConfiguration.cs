using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Realtime_ToDo_Web_API.Startup;

public static class SwaggerConfiguration
{
	public static IServiceCollection RegisterSwagger(this IServiceCollection services, string versionString) 
	{
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc(versionString, new OpenApiInfo
			{
				Version = versionString,
				Title = "Realtime-ToDo-Web-API",
				Description = "A part of realtime web API documentation for creating collaborative todo list",
			});
			var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
		});

		return services;
	}
	public static WebApplication ConfigureSwagger(this WebApplication app, string versionString)
	{
		app.UseSwagger();
		app.UseSwaggerUI(options =>
		{
			options.SwaggerEndpoint($"/swagger/{versionString}/swagger.json", versionString);
			options.RoutePrefix = string.Empty;
		});

		return app;
	}
}
