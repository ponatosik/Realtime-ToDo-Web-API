namespace Realtime_ToDo_Web_API.Hubs;

public interface IUserConnectionStorage : ISet<string> { }
public class UserConnectionStorage : HashSet<string> , IUserConnectionStorage { }
