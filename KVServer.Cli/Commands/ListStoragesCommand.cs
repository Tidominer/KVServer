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
        try
        {
            var storages = await _storageService.GetAllStoragesAsync();
            var storageList = storages.ToList();

            if (!storageList.Any())
            {
                Console.WriteLine("No storages found.");
                return 0;
            }

            Console.WriteLine("Available Storages:");
            Console.WriteLine();

            foreach (var storage in storageList)
            {
                Console.WriteLine($"ID: {storage.Id} | Name: {storage.Name} | Created: {storage.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"     Token: {storage.AccessToken}");
                Console.WriteLine();
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}