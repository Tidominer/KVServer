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

            Console.WriteLine($"Storage:  {storage.Name} (ID: {storage.Id})");
            Console.WriteLine("This will permanently delete the storage and all its keys.");
            Console.Write("Are you sure? (y/N): ");
            var answer = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (answer != "y" && answer != "yes")
            {
                Console.WriteLine("Cancelled.");
                return 0;
            }

            await _storageService.DeleteStorageAsync(storage.Id);
            Console.WriteLine($"Storage '{storage.Name}' deleted.");

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
        Console.WriteLine("Delete a storage and all its keys.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli storage delete <id|name>");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <id|name>    Numeric ID or name of the storage to delete");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli storage delete 1");
        Console.WriteLine("  kvserver-cli storage delete MyApp");
    }
}
