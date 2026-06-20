using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class DeleteStorageCommand : ICommand
{
    private readonly IStorageService _storageService;

    public DeleteStorageCommand(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Error: Storage ID is required");
            Console.Error.WriteLine("Usage: kvserver-cli storage delete <id>");
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

            await _storageService.DeleteStorageAsync(storageId);

            Console.WriteLine($"Storage '{storage.Name}' (ID: {storageId}) deleted successfully.");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}