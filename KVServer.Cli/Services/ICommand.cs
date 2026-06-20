namespace KVServer.Cli.Services;

public interface ICommand
{
    Task<int> ExecuteAsync(string[] args);
}