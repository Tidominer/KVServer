using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class RegenerateTokenCommand : ICommand
{
    private readonly IStorageService _storageService;
    private readonly ITokenService _tokenService;
    private readonly IEncryptionService _encryptionService;

    public RegenerateTokenCommand(
        IStorageService storageService,
        ITokenService tokenService,
        IEncryptionService encryptionService)
    {
        _storageService = storageService;
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

            storage.AccessToken = _tokenService.GenerateToken(storage.Id);
            storage.Salt = _encryptionService.GenerateSalt();
            await _storageService.UpdateStorageAsync(storage);

            Console.WriteLine($"Token regenerated for storage '{storage.Name}' (ID: {storage.Id}).");
            Console.WriteLine($"New Token: {storage.AccessToken}");
            Console.WriteLine();
            Console.WriteLine("WARNING: All existing encrypted values are now inaccessible.");
            Console.WriteLine("         The old token can no longer decrypt them.");

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
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli token regenerate <id|name>");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <id|name>    Numeric ID or name of the storage");
        Console.WriteLine();
        Console.WriteLine("WARNING: Regenerating a token invalidates all existing encrypted values.");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli token regenerate 1");
        Console.WriteLine("  kvserver-cli token regenerate MyApp");
    }
}
