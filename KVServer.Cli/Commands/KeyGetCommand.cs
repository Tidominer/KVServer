using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class KeyGetCommand : ICommand
{
    private readonly IStorageService _storageService;
    private readonly IKeyService _keyService;
    private readonly IEncryptionService _encryptionService;

    public KeyGetCommand(IStorageService storageService, IKeyService keyService, IEncryptionService encryptionService)
    {
        _storageService = storageService;
        _keyService = keyService;
        _encryptionService = encryptionService;
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

        var token = args[0];
        var keyName = args[1];

        // Parse optional --version N flag
        int? versionNumber = null;
        for (var i = 2; i < args.Length - 1; i++)
        {
            if (args[i] is "--version" or "-V" && int.TryParse(args[i + 1], out var v))
            {
                versionNumber = v;
                break;
            }
        }

        var storage = await _storageService.GetStorageByTokenAsync(token);
        if (storage == null)
        {
            Console.Error.WriteLine("Error: Invalid token or storage not found.");
            return 1;
        }

        string? value;

        if (versionNumber.HasValue)
        {
            var entry = await _keyService.GetKeyVersionAsync(storage.Id, keyName, versionNumber.Value);
            if (entry == null)
            {
                Console.Error.WriteLine($"Error: Key '{keyName}' version {versionNumber} not found.");
                return 1;
            }
            var encKey = _encryptionService.DeriveKey(storage.AccessToken, storage.Salt);
            value = _encryptionService.Decrypt(entry.EncryptedValue, entry.IV, encKey);
        }
        else
        {
            value = await _keyService.GetKeyValueAsync(storage.Id, keyName, token);
            if (value == null)
            {
                Console.Error.WriteLine($"Error: Key '{keyName}' not found.");
                return 1;
            }
        }

        Console.WriteLine(value);
        return 0;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Get the value of a key.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli key get <token> <key> [--version N]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <token>       Access token of the storage");
        Console.WriteLine("  <key>         Key name");
        Console.WriteLine("  --version N   Retrieve a specific version instead of the latest");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli key get kv_1_abc123 db.host");
        Console.WriteLine("  kvserver-cli key get kv_1_abc123 db.host --version 2");
    }
}
