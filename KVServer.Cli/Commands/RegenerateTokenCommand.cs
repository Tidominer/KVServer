using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class RegenerateTokenCommand : ICommand
{
    private readonly IStorageService _storageService;
    private readonly IKeyService _keyService;
    private readonly ITokenService _tokenService;
    private readonly IEncryptionService _encryptionService;

    public RegenerateTokenCommand(
        IStorageService storageService,
        IKeyService keyService,
        ITokenService tokenService,
        IEncryptionService encryptionService)
    {
        _storageService = storageService;
        _keyService = keyService;
        _tokenService = tokenService;
        _encryptionService = encryptionService;
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
            Console.Error.WriteLine("Error: Storage ID or name is required.");
            ShowHelp();
            return 1;
        }

        try
        {
            var storage = int.TryParse(args[0], out var id)
                ? await _storageService.GetStorageByIdAsync(id)
                : await _storageService.GetStorageByNameAsync(args[0]);

            if (storage == null)
            {
                Console.Error.WriteLine($"Error: No storage found matching '{args[0]}'.");
                return 1;
            }

            Console.WriteLine($"Storage: {storage.Name} (ID: {storage.Id})");
            Console.WriteLine("This will generate a new token and re-encrypt all key values.");
            Console.Write("Are you sure? (y/N): ");
            var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (answer != "y" && answer != "yes")
            {
                Console.WriteLine("Cancelled.");
                return 0;
            }

            // Derive old encryption key before changing anything
            var oldEncKey = _encryptionService.DeriveKey(storage.AccessToken, storage.Salt);

            // Generate new credentials
            var newToken = _tokenService.GenerateToken(storage.Id);
            var newSalt  = _encryptionService.GenerateSalt();
            var newEncKey = _encryptionService.DeriveKey(newToken, newSalt);

            // Re-encrypt all version entries with the new key
            Console.Write("Re-encrypting values... ");
            await _keyService.ReEncryptAllAsync(storage.Id, oldEncKey, newEncKey);
            Console.WriteLine("done.");

            // Update the storage token and salt
            storage.AccessToken = newToken;
            storage.Salt = newSalt;
            await _storageService.UpdateStorageAsync(storage);

            Console.WriteLine();
            Console.WriteLine("Token regenerated successfully.");
            Console.WriteLine($"New Token: {newToken}");
            Console.WriteLine();
            Console.WriteLine("All existing values have been re-encrypted with the new token.");

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
        Console.WriteLine("Regenerate the access token for a storage.");
        Console.WriteLine("All existing encrypted values are automatically re-encrypted.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli token regenerate <id|name>");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <id|name>    Numeric ID or name of the storage");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli token regenerate 1");
        Console.WriteLine("  kvserver-cli token regenerate MyApp");
    }
}
