namespace Realtime_ToDo_Web_API.Hubs;

public interface IUserConnectionStorage : IDictionary<string, int> 
{
    public int? GetGroupId(string connectionId);
    public string GetGroupName(string connectionId);
}
public class UserConnectionStorage : Dictionary<string, int> , IUserConnectionStorage 
{
    public int? GetGroupId(string connectionId) => this[connectionId];
    public string GetGroupName(string connectionId) => $"WorkspaceGroup:{this[connectionId]}";

}