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
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Error: Storage name is required");
            Console.Error.WriteLine("Usage: kvserver-cli storage create <name>");
            return 1;
        }

        var name = args[0];

        try
        {
            var storage = await _storageService.CreateStorageAsync(name);

            Console.WriteLine("Storage created successfully!");
            Console.WriteLine($"Name: {storage.Name}");
            Console.WriteLine($"Access Token: {storage.AccessToken}");
            Console.WriteLine();
            Console.WriteLine("⚠️  Save this token securely - it won't be shown again!");
            Console.WriteLine("   Use this token to access the storage via web UI or API.");

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
}