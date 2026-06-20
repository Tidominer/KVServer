using KVServer.Cli.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KVServer.Cli.Commands;

public class KeyCommandParser : ICommand
{
    private readonly IServiceProvider _serviceProvider;

    public KeyCommandParser(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length == 0 || args[0].ToLowerInvariant() is "help" or "--help" or "-h")
        {
            ShowHelp();
            return args.Length == 0 ? 1 : 0;
        }

        var subCommand = args[0].ToLowerInvariant();
        var subArgs = args.Skip(1).ToArray();

        ICommand? command = subCommand switch
        {
            "list"    => _serviceProvider.GetRequiredService<KeyListCommand>(),
            "get"     => _serviceProvider.GetRequiredService<KeyGetCommand>(),
            "set"     => _serviceProvider.GetRequiredService<KeySetCommand>(),
            "delete"  => _serviceProvider.GetRequiredService<KeyDeleteCommand>(),
            "history" => _serviceProvider.GetRequiredService<KeyHistoryCommand>(),
            _ => null
        };

        if (command == null)
        {
            Console.Error.WriteLine($"Unknown key subcommand: '{subCommand}'.");
            ShowHelp();
            return 1;
        }

        return await command.ExecuteAsync(subArgs);
    }

    internal static void ShowHelp()
    {
        Console.WriteLine("Manage keys within a storage.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli key <subcommand> <token> [arguments]");
        Console.WriteLine();
        Console.WriteLine("Subcommands:");
        Console.WriteLine("  list    <token>              List all keys and their versions");
        Console.WriteLine("  get     <token> <key>        Get the current value of a key");
        Console.WriteLine("  set     <token> <key> <val>  Create or update a key (use '-' for stdin)");
        Console.WriteLine("  delete  <token> <key>        Delete a key and all its versions");
        Console.WriteLine("  history <token> <key>        Show the version history of a key");
        Console.WriteLine("  help                         Show this help");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli key list    kv_1_abc123");
        Console.WriteLine("  kvserver-cli key get     kv_1_abc123 db.host");
        Console.WriteLine("  kvserver-cli key get     kv_1_abc123 db.host --version 2");
        Console.WriteLine("  kvserver-cli key set     kv_1_abc123 db.host localhost");
        Console.WriteLine("  kvserver-cli key delete  kv_1_abc123 db.host");
        Console.WriteLine("  kvserver-cli key history kv_1_abc123 db.host");
    }
}
