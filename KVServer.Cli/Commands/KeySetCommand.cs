using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class KeySetCommand : ICommand
{
    private readonly IStorageService _storageService;
    private readonly IKeyService _keyService;

    public KeySetCommand(IStorageService storageService, IKeyService keyService)
    {
        _storageService = storageService;
        _keyService = keyService;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length > 0 && args[0].ToLowerInvariant() is "help" or "--help" or "-h")
        {
            ShowHelp();
            return 0;
        }

        if (args.Length < 3)
        {
            Console.Error.WriteLine("Error: token, key name, and value are required.");
            ShowHelp();
            return 1;
        }

        var token   = args[0];
        var keyName = args[1];
        var value   = args[2] == "-" ? Console.In.ReadToEnd() : args[2];

        var storage = await _storageService.GetStorageByTokenAsync(token);
        if (storage == null)
        {
            Console.Error.WriteLine("Error: Invalid token or storage not found.");
            return 1;
        }

        try
        {
            var (_, created) = await _keyService.UpsertKeyAsync(storage.Id, keyName, value, "cli");
            Console.WriteLine(created ? $"Created '{keyName}'." : $"Updated '{keyName}'.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Create or update a key-value pair.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli key set <token> <key> <value>");
        Console.WriteLine("       kvserver-cli key set <token> <key> -   (read value from stdin)");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <token>    Access token of the storage");
        Console.WriteLine("  <key>      Key name");
        Console.WriteLine("  <value>    Value to store, or '-' to read from stdin");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli key set kv_1_abc123 db.host localhost");
        Console.WriteLine("  cat config.json | kvserver-cli key set kv_1_abc123 app.config -");
    }
}
