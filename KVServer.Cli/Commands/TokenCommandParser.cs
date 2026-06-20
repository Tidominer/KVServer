using KVServer.Cli.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KVServer.Cli.Commands;

public class TokenCommandParser : ICommand
{
    private readonly IServiceProvider _serviceProvider;

    public TokenCommandParser(IServiceProvider serviceProvider)
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
            "regenerate" => _serviceProvider.GetRequiredService<RegenerateTokenCommand>(),
            _ => null
        };

        if (command == null)
        {
            Console.Error.WriteLine($"Unknown token subcommand: '{subCommand}'.");
            ShowHelp();
            return 1;
        }

        return await command.ExecuteAsync(subArgs);
    }

    internal static void ShowHelp()
    {
        Console.WriteLine("Manage access tokens for storages.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli token <subcommand> [arguments]");
        Console.WriteLine();
        Console.WriteLine("Subcommands:");
        Console.WriteLine("  regenerate <id|name>    Generate a new access token for a storage");
        Console.WriteLine("  help                    Show this help");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli token regenerate 1");
        Console.WriteLine("  kvserver-cli token regenerate MyApp");
    }
}
