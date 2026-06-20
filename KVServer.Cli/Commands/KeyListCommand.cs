using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class KeyListCommand : ICommand
{
    private readonly IStorageService _storageService;
    private readonly IKeyService _keyService;

    public KeyListCommand(IStorageService storageService, IKeyService keyService)
    {
        _storageService = storageService;
        _keyService = keyService;
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
            Console.Error.WriteLine("Error: token is required.");
            ShowHelp();
            return 1;
        }

        var storage = await _storageService.GetStorageByTokenAsync(args[0]);
        if (storage == null)
        {
            Console.Error.WriteLine("Error: Invalid token or storage not found.");
            return 1;
        }

        var keys = (await _keyService.GetKeysByStorageIdAsync(storage.Id)).ToList();

        if (keys.Count == 0)
        {
            Console.WriteLine($"No keys in '{storage.Name}'.");
            return 0;
        }

        Console.WriteLine($"{keys.Count} key(s) in '{storage.Name}':");
        Console.WriteLine();
        Console.WriteLine($"  {"KEY",-40}  {"VER",4}  LAST MODIFIED");
        Console.WriteLine($"  {new string('-', 40)}  {new string('-', 4)}  {new string('-', 20)}");

        foreach (var k in keys)
        {
            var version = k.Versions?.Count ?? 0;
            var modified = k.Versions?.Any() == true
                ? k.Versions.Max(v => v.CreatedAt).ToString("yyyy-MM-dd HH:mm")
                : k.CreatedAt.ToString("yyyy-MM-dd HH:mm");

            Console.WriteLine($"  {k.KeyName,-40}  v{version,-3}  {modified}");
        }

        return 0;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("List all keys in a storage.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli key list <token>");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <token>    Access token of the storage");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli key list kv_1_abc123");
    }
}
