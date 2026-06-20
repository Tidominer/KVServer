using KVServer.Cli.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KVServer.Cli.Commands;

public class StorageCommandParser : ICommand
{
    private readonly IServiceProvider _serviceProvider;

    public StorageCommandParser(IServiceProvider serviceProvider)
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
            "create" => _serviceProvider.GetRequiredService<CreateStorageCommand>(),
            "list"   => _serviceProvider.GetRequiredService<ListStoragesCommand>(),
            "delete" => _serviceProvider.GetRequiredService<DeleteStorageCommand>(),
            _ => null
        };

        if (command == null)
        {
            Console.Error.WriteLine($"Unknown storage subcommand: '{subCommand}'.");
            ShowHelp();
            return 1;
        }

        return await command.ExecuteAsync(subArgs);
    }

    internal static void ShowHelp()
    {
        Console.WriteLine("Manage storages.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli storage <subcommand> [arguments]");
        Console.WriteLine();
        Console.WriteLine("Subcommands:");
        Console.WriteLine("  create <name>        Create a new storage and generate an access token");
        Console.WriteLine("  list                 List all storages with IDs and tokens");
        Console.WriteLine("  delete <id|name>     Delete a storage by ID or name");
        Console.WriteLine("  help                 Show this help");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli storage create MyApp");
        Console.WriteLine("  kvserver-cli storage list");
        Console.WriteLine("  kvserver-cli storage delete 1");
        Console.WriteLine("  kvserver-cli storage delete MyApp");
    }
}
