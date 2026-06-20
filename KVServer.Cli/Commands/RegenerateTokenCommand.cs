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
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Error: Storage ID is required");
            Console.Error.WriteLine("Usage: kvserver-cli token regenerate <id>");
            return 1;
        }

        if (!int.TryParse(args[0], out var storageId))
        {
            Console.Error.WriteLine("Error: Invalid storage ID");
            return 1;
        }

        try
        {
            var storage = await _storageService.GetStorageByIdAsync(storageId);
            if (storage == null)
            {
                Console.Error.WriteLine($"Error: Storage with ID {storageId} not found");
                return 1;
            }

            // Generate new token and salt
            var newToken = _tokenService.GenerateToken(storage.Id);
            var newSalt = _encryptionService.GenerateSalt();

            storage.AccessToken = newToken;
            storage.Salt = newSalt;
            await _storageService.CreateStorageAsync(storage.Name);

            Console.WriteLine($"Access token regenerated for storage '{storage.Name}'");
            Console.WriteLine($"New Token: {newToken}");
            Console.WriteLine();
            Console.WriteLine("⚠️  IMPORTANT: All existing encrypted values must be re-encrypted");
            Console.WriteLine("   with the new token. The old encrypted values are no longer accessible.");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}