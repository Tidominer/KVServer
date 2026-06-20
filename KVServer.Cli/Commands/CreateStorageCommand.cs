using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class CreateStorageCommand : ICommand
{
    private readonly IStorageService _storageService;

    public CreateStorageCommand(IStorageService storageService)
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
            Console.Error.WriteLine("Error: Storage name is required.");
            ShowHelp();
            return 1;
        }

        try
        {
            var storage = await _storageService.CreateStorageAsync(args[0]);

            Console.WriteLine("Storage created successfully!");
            Console.WriteLine($"ID:           {storage.Id}");
            Console.WriteLine($"Name:         {storage.Name}");
            Console.WriteLine($"Access Token: {storage.AccessToken}");
            Console.WriteLine();
            Console.WriteLine("Save this token securely — it will not be shown again.");

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
        Console.WriteLine("Create a new storage and generate an access token.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli storage create <name>");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <name>    Name of the new storage (must be unique)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli storage create MyApp");
        Console.WriteLine("  kvserver-cli storage create ProductionConfig");
    }
}
