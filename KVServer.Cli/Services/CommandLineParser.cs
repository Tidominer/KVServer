namespace KVServer.Cli.Services;

public class CommandLineParser
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public void RegisterCommand(string command, ICommand commandHandler)
    {
        _commands[command] = commandHandler;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        if (!_commands.ContainsKey(command))
        {
            Console.Error.WriteLine($"Unknown command: {command}");
            ShowHelp();
            return 1;
        }

        var commandArgs = args.Skip(1).ToArray();
        return await _commands[command].ExecuteAsync(commandArgs);
    }

    private void ShowHelp()
    {
        Console.WriteLine("KVServer CLI - Key-Value Storage Management Tool");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli <command> [arguments]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  storage create <name>       Create a new storage");
        Console.WriteLine("  storage list                List all storages");
        Console.WriteLine("  storage delete <id>         Delete a storage");
        Console.WriteLine("  token regenerate <id>       Regenerate access token");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli storage create MyStorage");
        Console.WriteLine("  kvserver-cli storage list");
        Console.WriteLine("  kvserver-cli storage delete 1");
    }
}