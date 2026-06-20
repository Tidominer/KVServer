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
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Error: Storage command requires a subcommand");
            Console.Error.WriteLine("Usage: kvserver-cli storage <create|list|delete> [arguments]");
            return 1;
        }

        var subCommand = args[0].ToLowerInvariant();
        var subArgs = args.Skip(1).ToArray();

        ICommand command = subCommand switch
        {
            "create" => _serviceProvider.GetRequiredService<CreateStorageCommand>(),
            "list" => _serviceProvider.GetRequiredService<ListStoragesCommand>(),
            "delete" => _serviceProvider.GetRequiredService<DeleteStorageCommand>(),
            _ => throw new InvalidOperationException($"Unknown storage subcommand: {subCommand}")
        };

        return await command.ExecuteAsync(subArgs);
    }
}