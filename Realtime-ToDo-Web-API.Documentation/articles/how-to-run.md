# How to run and host this API

## About this API

The Realtime ToDo Web API provides a backend for managing and collaborating on tasks in real-time. It uses the SignalR technology to enable real-time updates and notifications across multiple clients.

The Realtime ToDo Web API offers two main ways of integration:

1. **REST API:** The REST API provides a straightforward approach to interact with the backend. It allows you to perform CRUD (Create, Read, Update, Delete) operations on workspaces and tasks. You can use any HTTP client, such as Postman or cURL, to send requests to the API endpoints.

2. **SignalR Hubs:** The SignalR Hub introduces real-time communication capabilities. By connecting to the Workspaces Hub and Todolist Hub, you can receive live changes to workspaces or tasks. This enables you to build interactive and collaborative applications that instantly reflect changes made by other users.

## Requirements

1. ASP .NET Core SDK: Install the latest version of .NET Core SDK, which can be downloaded from the official [.NET website.](https://dotnet.microsoft.com/download)
2. Database: Set up a SQL database server and ensure that you have the necessary connection details, including the server address, credentials, and database name.

## Run this project

# [Command line](#tab/CLI)

1. Clone the repository:  ``` git clone https://github.com/ponatosik/Realtime-ToDo-Web-API.git ``` 
2. Open it: ``` cd .\Realtime-ToDo-Web-API\ ```
3. Restore dependencies: ``` dotnet restore ```
4. [Connect to your database](#connect-to-a-database). Or use [in-memory-database](#in-memory-database-for-testing-purposes)
5. Run this project: ``` dotnet run .\Realtime-ToDo-Web-API\Realtime-ToDo-Web-API.csproj ``` 
6. Open hosted project on localhost (see console output)

# [Visual Studio](#tab/VS)

1. Open Visual Studio
2. Clone the repository: https://github.com/ponatosik/Realtime-ToDo-Web-API.git
3. [Connect to your database](#connect-to-a-database). Or use [in-memory-database](#in-memory-database-for-testing-purposes)
4. Run this project (F5 key). It must automatically open localhost in a browser

---

## Connect to a database

To store and retrieve data, the Realtime ToDo Web API requires a connection to a database. In this section, we'll guide you through the process of setting up the database connection.

### In memory database for testing purposes

An in-memory database allows you to simulate a database environment without the need for an actual database server. It is particularly useful for running unit tests or performing quick local testing.

To use an in-memory database in the Realtime ToDo Web API open Program.cs file and find this line 
```
builder.Services.AddDbContext<TodoListContext>(options =>
```
Then uncomment  
```
options.UseInMemoryDatabase("TodoListInMemoryDatabase")
``` 
and comment 
```
options.UseSqlServer(builder.Configuration.GetConnectionString("TodoListDB"))
```
Now you can run this API without having an actual database.

### Configuration Files

You can store database connection details  in the appsettings.json file. Open the appsettings.json file in your project and locate the "ConnectionStrings" section.

``` 
  "ConnectionStrings": {
    //"TodoListDB": "Place your DB connection string here"
  }
``` 

Uncomment it and place the actual connection string for your database. The connection string contains information about the database provider, server location, credentials, and other parameters required to establish a connection.  


###  Dotnet User Secrets

For development purposes, you can also store the connection string securely using the dotnet user secrets feature. This allows you to keep sensitive information separate from your code repository and provides an extra layer of security.

To use dotnet user secrets, open a terminal or command prompt and navigate to the root folder of your project. Run the following command:

```
dotnet user-secrets init
dotnet user-secrets set ConnectionStrings:TodoListDB "YourConnectionString"
```

Replace "YourConnectionString" with your actual connection string.

> [!TIP]
> It is preferred  to store your connection strings in User Secrets over Configuration Files, as it doesn't expose your connection details, including credentials.

### Migrating the Database

Once you have set up the database connection for the Realtime ToDo Web API, you need to apply any pending database migrations. The migrations ensure that the database schema is up to date with the latest changes in your code.

To apply the migrations, you can use the dotnet ef database update command. Here's how you can run this command:

1. Open a terminal or command prompt.
2. Navigate to the root folder of your project.
3. Run the following command: 
```
dotnet ef database update
```

This command will apply any pending migrations and update the database schema accordingly. It reads the migrations defined in your project and applies them to the connected database.

Make sure you have the Entity Framework Core tools installed (dotnet-ef) for the dotnet command to recognize the ef command. If you don't have them installed, you can install them by running the following command:
