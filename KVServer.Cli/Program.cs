using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KVServer.Cli.Commands;
using KVServer.Cli.Services;
using KVServer.Core.Services;
using KVServer.Core.Repositories;
using KVServer.Infrastructure.Data;
using KVServer.Infrastructure.Repositories;
using KVServer.Infrastructure.Services;

namespace KVServer.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // Get database path from configuration
        var dbPath = configuration["DbPath"];
        if (string.IsNullOrEmpty(dbPath))
        {
            // Default to ./data/kvserver.db if not specified
            var dataDirectory = "./data";
            if (!Path.IsPathRooted(dataDirectory))
            {
                var basePath = Directory.GetCurrentDirectory();
                dataDirectory = Path.Combine(basePath, dataDirectory);
            }
            Directory.CreateDirectory(dataDirectory);
            dbPath = Path.Combine(dataDirectory, "kvserver.db");
        }

        // Ensure the directory for the database file exists
        var dbDirectory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
            Console.WriteLine($"Created database directory: {dbDirectory}");
        }

        // Construct connection string from DbPath
        var connectionString = $"Data Source={dbPath}";
        Console.Error.WriteLine($"Database: {dbPath}");

        // Setup dependency injection
        var serviceProvider = new ServiceCollection()
            .AddDbContext<KVServerDbContext>(options =>
                options.UseSqlite(connectionString))
            .AddScoped<IStorageRepository, StorageRepository>()
            .AddScoped<IKeyRepository, KeyRepository>()
            .AddScoped<IVersionRepository, VersionRepository>()
            .AddScoped<IEncryptionService, EncryptionService>()
            .AddScoped<ITokenService, TokenService>()
            .AddScoped<IStorageService, StorageService>()
            .AddScoped<IKeyService, KeyService>()
            .AddTransient<CreateStorageCommand>()
            .AddTransient<ListStoragesCommand>()
            .AddTransient<DeleteStorageCommand>()
            .AddTransient<RegenerateTokenCommand>()
            .AddTransient<KeyListCommand>()
            .AddTransient<KeyGetCommand>()
            .AddTransient<KeySetCommand>()
            .AddTransient<KeyDeleteCommand>()
            .AddTransient<KeyHistoryCommand>()
            .AddTransient<KeyExportCommand>()
            .AddTransient<KeyImportCommand>()
            .BuildServiceProvider();

        // Ensure database is created
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<KVServerDbContext>();
        dbContext.Database.EnsureCreated();

        // Setup command parser
        var parser = new CommandLineParser();
        parser.RegisterCommand("storage", new StorageCommandParser(serviceProvider));
        parser.RegisterCommand("token", new TokenCommandParser(serviceProvider));
        parser.RegisterCommand("key", new KeyCommandParser(serviceProvider));

        // Execute command
        try
        {
            return await parser.ExecuteAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 1;
        }
    }
}