using System.Text.Json;
using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class KeyImportCommand : ICommand
{
    private readonly IStorageService _storageService;
    private readonly IKeyService _keyService;

    public KeyImportCommand(IStorageService storageService, IKeyService keyService)
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
            Console.Error.WriteLine("Error: token and file are required.");
            ShowHelp();
            return 1;
        }

        var token    = args[0];
        var filePath = args[1];

        var storage = await _storageService.GetStorageByTokenAsync(token);
        if (storage == null)
        {
            Console.Error.WriteLine("Error: Invalid token or storage not found.");
            return 1;
        }

        string json;
        if (filePath == "-")
        {
            json = await Console.In.ReadToEndAsync();
        }
        else
        {
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"Error: File not found: '{filePath}'");
                return 1;
            }
            json = await File.ReadAllTextAsync(filePath);
        }

        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Error: Invalid JSON — {ex.Message}");
            return 1;
        }

        if (!doc.RootElement.TryGetProperty("keys", out var keysEl) || keysEl.ValueKind != JsonValueKind.Array)
        {
            Console.Error.WriteLine("Error: JSON must contain a 'keys' array.");
            return 1;
        }

        int created = 0, updated = 0, failed = 0;

        foreach (var entry in keysEl.EnumerateArray())
        {
            if (!entry.TryGetProperty("key", out var kp) || !entry.TryGetProperty("value", out var vp))
            {
                Console.Error.WriteLine("  Warning: skipping entry missing 'key' or 'value'.");
                failed++;
                continue;
            }

            var keyName = kp.GetString()!;
            var value   = vp.GetString()!;

            try
            {
                var (_, isNew) = await _keyService.UpsertKeyAsync(storage.Id, keyName, value, "cli-import");
                if (isNew) created++; else updated++;
                Console.Error.WriteLine($"  {(isNew ? "Created" : "Updated")} '{keyName}'.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  Error on '{keyName}': {ex.Message}");
                failed++;
            }
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine($"Import complete: {created} created, {updated} updated, {failed} failed.");
        return (created + updated == 0 && failed > 0) ? 1 : 0;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Import keys into a storage from a JSON file.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli key import <token> <file>");
        Console.WriteLine("       kvserver-cli key import <token> -   (read from stdin)");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <token>    Access token of the storage");
        Console.WriteLine("  <file>     Path to a JSON export file, or '-' for stdin");
        Console.WriteLine();
        Console.WriteLine("Existing keys are updated; new keys are created.");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli key import kv_1_abc123 backup.json");
        Console.WriteLine("  cat backup.json | kvserver-cli key import kv_1_abc123 -");
    }
}
