# API for collaborative todo lists

 This repository contains the backend code for a real-time ToDo web application. It is built using ASP.NET Core and utilizes SignalR for real-time collaborative functionality.

## Project links

* [Hosted API](https://realtimetodowebapi.azurewebsites.net/) (this repo)
* [Documentation](https://ponatosik.github.io/Realtime-ToDo-Web-API/) (this repo)
* [Frontend example](https://todo-list-masmits.vercel.app/) ([another repo](https://github.com/MasMits/TodoList))

## Requirements

* ASP.net core 7.0

## How to run

1. Clone this repo:  ``` git clone https://github.com/ponatosik/Realtime-ToDo-Web-API.git ``` 
2. Open it: ``` cd .\Realtime-ToDo-Web-API\ ```
3. Restore dependencies: ``` dotnet restore ```
4. Provide your connection string, or use in-memory database. Uncomment  ```options.UseInMemoryDatabase("TodoListInMemoryDatabase")``` in Program.cs file
5. Run this project: ``` dotnet run --project .\Realtime-ToDo-Web-API\Realtime-ToDo-Web-API.csproj ``` 
6. Open hosted project on localhost (see console output)

## Stack

* EntityFrameworkCore
* SigalR
* Swagger UI (Swashbuckle )
* Docfx
