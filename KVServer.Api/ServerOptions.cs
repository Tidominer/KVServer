using Microsoft.Extensions.Logging;

namespace KVServer.Api;

public class ServerOptions
{
    public bool NoWeb { get; init; }
    public bool ReadOnly { get; init; }
    public bool NoCors { get; init; }
    public int? Port { get; init; }
    public string Bind { get; init; } = "localhost";
    public string? DbPath { get; init; }
    public LogLevel LogLevel { get; init; } = LogLevel.Information;
    public int RateLimit { get; init; } = 3;
    public string[] RemainingArgs { get; init; } = [];

    public static ServerOptions Parse(string[] args, IConfiguration? config = null)
    {
        bool noWeb    = config?.GetValue<bool>("Server:NoWeb")     ?? false;
        bool readOnly = config?.GetValue<bool>("Server:ReadOnly")  ?? false;
        bool noCors   = config?.GetValue<bool>("Server:NoCors")    ?? false;
        int? port     = config?.GetValue<int?>("Server:Port");
        string bind   = config?["Server:Bind"]                     ?? "localhost";
        string? dbPath = config?["Server:DbPath"];
        var logLevel  = config?.GetValue<LogLevel>("Server:LogLevel") ?? LogLevel.Information;
        int rateLimit = config?.GetValue<int>("Server:RateLimit")  ?? 3;
        var remaining = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--no-web":
                    noWeb = true;
                    break;
                case "--read-only":
                    readOnly = true;
                    break;
                case "--no-cors":
                    noCors = true;
                    break;
                case "--port":
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var p) && p > 0 && p <= 65535)
                        port = p;
                    else
                    {
                        Console.Error.WriteLine("Error: --port requires a valid port number (1-65535).");
                        Environment.Exit(1);
                    }
                    i++;
                    break;
                case "--bind":
                    if (i + 1 < args.Length)
                        bind = args[i + 1];
                    else
                    {
                        Console.Error.WriteLine("Error: --bind requires an address (e.g. localhost, 0.0.0.0, 127.0.0.1).");
                        Environment.Exit(1);
                    }
                    i++;
                    break;
                case "--db":
                    if (i + 1 < args.Length)
                        dbPath = args[i + 1];
                    else
                    {
                        Console.Error.WriteLine("Error: --db requires a file path.");
                        Environment.Exit(1);
                    }
                    i++;
                    break;
                case "--log-level":
                    if (i + 1 < args.Length && Enum.TryParse<LogLevel>(args[i + 1], ignoreCase: true, out var ll))
                        logLevel = ll;
                    else
                    {
                        Console.Error.WriteLine("Error: --log-level requires one of: Trace, Debug, Information, Warning, Error, Critical, None.");
                        Environment.Exit(1);
                    }
                    i++;
                    break;
                case "--rate-limit":
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var rl) && rl > 0)
                        rateLimit = rl;
                    else
                    {
                        Console.Error.WriteLine("Error: --rate-limit requires a positive integer (failed attempts per minute before lockout).");
                        Environment.Exit(1);
                    }
                    i++;
                    break;
                default:
                    remaining.Add(args[i]);
                    break;
            }
        }

        return new ServerOptions
        {
            NoWeb = noWeb,
            ReadOnly = readOnly,
            NoCors = noCors,
            Port = port,
            Bind = bind,
            DbPath = dbPath,
            LogLevel = logLevel,
            RateLimit = rateLimit,
            RemainingArgs = [.. remaining]
        };
    }

    public void PrintStartupInfo()
    {
        Console.WriteLine($"  No-Web:     {NoWeb}");
        Console.WriteLine($"  Read-Only:  {ReadOnly}");
        Console.WriteLine($"  No-CORS:    {NoCors}");
        Console.WriteLine($"  Log Level:  {LogLevel}");
        Console.WriteLine($"  Rate Limit: {RateLimit} failed attempts/min");
        Console.WriteLine($"  Bind:       {Bind}");
        if (Port.HasValue)   Console.WriteLine($"  Port:       {Port}");
        if (DbPath != null)  Console.WriteLine($"  DB Path:    {DbPath}");
    }
}
