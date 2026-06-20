using System.Text.Json;
using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class KeyExportCommand : ICommand
{
    private readonly IStorageService _storageService;
    private readonly IKeyService _keyService;

    public KeyExportCommand(IStorageService storageService, IKeyService keyService)
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

        if (args.Length == 0)
        {
            Console.Error.WriteLine("Error: token is required.");
            ShowHelp();
            return 1;
        }

        var token = args[0];
        string? outputFile = null;
        for (var i = 1; i < args.Length - 1; i++)
        {
            if (args[i] is "--output" or "-o") { outputFile = args[i + 1]; break; }
        }

        var storage = await _storageService.GetStorageByTokenAsync(token);
        if (storage == null)
        {
            Console.Error.WriteLine("Error: Invalid token or storage not found.");
            return 1;
        }

        var keys = (await _keyService.GetKeysByStorageIdAsync(storage.Id)).ToList();
        Console.Error.WriteLine($"Exporting {keys.Count} key(s) from '{storage.Name}'...");

        var exportKeys = new List<object>();
        var failed = 0;

        foreach (var key in keys)
        {
            var value = await _keyService.GetKeyValueAsync(storage.Id, key.KeyName, token);
            if (value == null)
            {
                Console.Error.WriteLine($"  Warning: could not decrypt '{key.KeyName}', skipping.");
                failed++;
                continue;
            }
            exportKeys.Add(new { key = key.KeyName, value });
        }

        var export = new
        {
            storage = storage.Name,
            exportedAt = DateTime.UtcNow.ToString("o"),
            keys = exportKeys
        };

        var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });

        if (outputFile != null)
        {
            await File.WriteAllTextAsync(outputFile, json);
            Console.Error.WriteLine($"Exported {exportKeys.Count} key(s) to '{outputFile}'.");
        }
        else
        {
            Console.WriteLine(json);
            Console.Error.WriteLine($"Exported {exportKeys.Count} key(s).");
        }

        if (failed > 0) Console.Error.WriteLine($"{failed} key(s) skipped due to decryption errors.");
        return failed > 0 ? 2 : 0;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Export all keys from a storage to JSON.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli key export <token> [--output <file>]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <token>           Access token of the storage");
        Console.WriteLine("  --output <file>   Write JSON to a file instead of stdout");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli key export kv_1_abc123 > backup.json");
        Console.WriteLine("  kvserver-cli key export kv_1_abc123 --output backup.json");
    }
}
