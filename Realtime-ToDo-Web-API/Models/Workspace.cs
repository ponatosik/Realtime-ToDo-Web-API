﻿namespace Realtime_ToDo_Web_API.Models;

public class Workspace
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<TodoTask>? Tasks { get; set; }
}