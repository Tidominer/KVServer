using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class ListStoragesCommand : ICommand
{
    private readonly IStorageService _storageService;

    public ListStoragesCommand(IStorageService storageService)
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

        try
        {
            var storages = (await _storageService.GetAllStoragesAsync()).ToList();

            if (storages.Count == 0)
            {
                Console.WriteLine("No storages found.");
                return 0;
            }

            Console.WriteLine($"{"ID",-4}  {"Name",-30}  {"Created",-20}  Token");
            Console.WriteLine(new string('-', 90));

            foreach (var s in storages)
                Console.WriteLine($"{s.Id,-4}  {s.Name,-30}  {s.CreatedAt:yyyy-MM-dd HH:mm:ss}  {s.AccessToken}");

            Console.WriteLine();
            Console.WriteLine($"{storages.Count} storage(s) total.");

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
        Console.WriteLine("List all storages with their IDs and access tokens.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli storage list");
    }
}
