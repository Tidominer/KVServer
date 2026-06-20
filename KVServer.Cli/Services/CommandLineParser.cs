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
            return 1;
        }

        var command = args[0].ToLowerInvariant();

        if (command is "help" or "--help" or "-h")
        {
            ShowHelp();
            return 0;
        }

        if (!_commands.TryGetValue(command, out var handler))
        {
            Console.Error.WriteLine($"Unknown command: '{command}'.");
            ShowHelp();
            return 1;
        }

        return await handler.ExecuteAsync(args.Skip(1).ToArray());
    }

    private static void ShowHelp()
    {
        Console.WriteLine("KVServer CLI — Key-Value Storage Management");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli <command> [subcommand] [arguments]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  storage    Manage storages (create, list, delete)");
        Console.WriteLine("  token      Manage access tokens (regenerate)");
        Console.WriteLine("  key        Manage keys (list, get, set, delete, history)");
        Console.WriteLine("  help       Show this help");
        Console.WriteLine();
        Console.WriteLine("Run 'kvserver-cli <command> help' for command-specific help.");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli storage create MyApp");
        Console.WriteLine("  kvserver-cli storage list");
        Console.WriteLine("  kvserver-cli key list    kv_1_abc123");
        Console.WriteLine("  kvserver-cli key get     kv_1_abc123 db.host");
        Console.WriteLine("  kvserver-cli key set     kv_1_abc123 db.host localhost");
        Console.WriteLine("  kvserver-cli key history kv_1_abc123 db.host");
    }
}
