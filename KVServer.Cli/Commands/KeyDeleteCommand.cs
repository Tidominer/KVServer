using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class KeyDeleteCommand : ICommand
{
    private readonly IStorageService _storageService;
    private readonly IKeyService _keyService;

    public KeyDeleteCommand(IStorageService storageService, IKeyService keyService)
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

        if (args.Length < 2)
        {
            Console.Error.WriteLine("Error: token and key name are required.");
            ShowHelp();
            return 1;
        }

        var token   = args[0];
        var keyName = args[1];

        var storage = await _storageService.GetStorageByTokenAsync(token);
        if (storage == null)
        {
            Console.Error.WriteLine("Error: Invalid token or storage not found.");
            return 1;
        }

        Console.WriteLine($"Key:      {keyName}");
        Console.WriteLine($"Storage:  {storage.Name}");
        Console.WriteLine("This will permanently delete the key and all its versions.");
        Console.Write("Are you sure? (y/N): ");
        var answer = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (answer != "y" && answer != "yes")
        {
            Console.WriteLine("Cancelled.");
            return 0;
        }

        try
        {
            await _keyService.DeleteKeyAsync(storage.Id, keyName);
            Console.WriteLine($"Deleted '{keyName}'.");
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Delete a key and all its versions.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli key delete <token> <key>");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <token>    Access token of the storage");
        Console.WriteLine("  <key>      Key name to delete");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli key delete kv_1_abc123 db.host");
    }
}
