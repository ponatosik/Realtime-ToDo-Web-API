using Microsoft.EntityFrameworkCore;
using Realtime_ToDo_Web_API.Data;

namespace Realtime_ToDo_Web_API.Startup;

public static class DatabaseSetup
{
	public static WebApplicationBuilder ConfigureDatabase(this WebApplicationBuilder builder) 
	{
		if (builder.Environment.IsDevelopment())
			builder.Services.ConfigureInMemoryDatabase();
		else
			builder.Services.ConfigureMySqlDatabase(builder.Configuration.GetConnectionString("TodoListDB")!);

		return builder;
	}

	private static IServiceCollection ConfigureMySqlDatabase(this IServiceCollection services, string connectionString) 
	{
		var mySqlVersion = new MySqlServerVersion(new Version(5, 7, 9));

		services.AddDbContext<TodoListContext>(options =>
			options.UseMySql(connectionString, mySqlVersion));

		return services;
	}

	private static IServiceCollection ConfigureInMemoryDatabase(this IServiceCollection services)
	{
		services.AddDbContext<TodoListContext>(options =>
			options.UseInMemoryDatabase("TodoListInMemoryDatabase"));

		return services;
	}

}
