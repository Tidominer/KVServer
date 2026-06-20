using KVServer.Cli.Services;
using KVServer.Core.Services;

namespace KVServer.Cli.Commands;

public class KeyHistoryCommand : ICommand
{
    private readonly IStorageService _storageService;
    private readonly IKeyService _keyService;

    public KeyHistoryCommand(IStorageService storageService, IKeyService keyService)
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

        if (args.Length < 2)
        {
            Console.Error.WriteLine("Error: token and key name are required.");
            ShowHelp();
            return 1;
        }

        var storage = await _storageService.GetStorageByTokenAsync(args[0]);
        if (storage == null)
        {
            Console.Error.WriteLine("Error: Invalid token or storage not found.");
            return 1;
        }

        var versions = (await _keyService.GetKeyHistoryAsync(storage.Id, args[1]))
            .OrderBy(v => v.VersionNumber)
            .ToList();

        if (versions.Count == 0)
        {
            Console.Error.WriteLine($"Error: Key '{args[1]}' not found.");
            return 1;
        }

        Console.WriteLine($"{versions.Count} version(s) of '{args[1]}' in '{storage.Name}':");
        Console.WriteLine();
        Console.WriteLine($"  {"VER",4}  {"CREATED",-20}  CREATED BY");
        Console.WriteLine($"  {new string('-', 4)}  {new string('-', 20)}  {new string('-', 15)}");

        foreach (var v in versions)
            Console.WriteLine($"  v{v.VersionNumber,-3}  {v.CreatedAt:yyyy-MM-dd HH:mm:ss}  {v.CreatedBy}");

        Console.WriteLine();
        Console.WriteLine("Use 'kvserver-cli key get <token> <key> --version N' to retrieve a specific value.");

        return 0;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Show the version history of a key.");
        Console.WriteLine();
        Console.WriteLine("Usage: kvserver-cli key history <token> <key>");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <token>    Access token of the storage");
        Console.WriteLine("  <key>      Key name");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  kvserver-cli key history kv_1_abc123 db.host");
    }
}
