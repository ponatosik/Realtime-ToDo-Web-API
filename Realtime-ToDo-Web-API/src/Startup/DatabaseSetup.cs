using Microsoft.EntityFrameworkCore;
using Realtime_ToDo_Web_API.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Realtime_ToDo_Web_API.Startup;

public static class DatabaseSetup
{
	public static WebApplicationBuilder ConfigureDatabase(this WebApplicationBuilder builder) 
	{
		if (builder.Environment.IsDevelopment())
			builder.Services.ConfigureInMemoryDatabase();
		else if (builder.Configuration.GetConnectionString("TodoListDB") is not null)
			builder.Services.ConfigureMySqlDatabase(builder.Configuration.GetConnectionString("TodoListDB")!);
		else if (Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb") is not null)
			builder.Services.ConfigureAzureMySqlInAppDatabase(Environment.GetEnvironmentVariable("MYSQLCONNSTR_localdb")!);

		return builder;
	}

	private static IServiceCollection ConfigureAzureMySqlInAppDatabase(this IServiceCollection services, string connectionString)
	{
		DbConnectionStringBuilder inDbConnectionStringBuilder = new DbConnectionStringBuilder();
		inDbConnectionStringBuilder.ConnectionString = connectionString;

		String database = (string)inDbConnectionStringBuilder["Database"];
		String sourceIp = ((string)inDbConnectionStringBuilder["Data Source"])[..^6];
		String sourcePort = ((string)inDbConnectionStringBuilder["Data Source"])[^5..];
		String user = (string)inDbConnectionStringBuilder["User Id"];
		String password = (string)inDbConnectionStringBuilder["Password"];

		DbConnectionStringBuilder outDbConnectionStringBuilder = new DbConnectionStringBuilder();

		outDbConnectionStringBuilder.Add("Data Source", sourceIp);
		outDbConnectionStringBuilder.Add("Port", sourcePort);
		outDbConnectionStringBuilder.Add("Database", database);
		outDbConnectionStringBuilder.Add("User Id", user);
		outDbConnectionStringBuilder.Add("Password", password);

		return ConfigureMySqlDatabase(services, outDbConnectionStringBuilder.ConnectionString);
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
