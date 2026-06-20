using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using KVServer.Api;
using KVServer.Api.Middleware;
using KVServer.Core.Repositories;
using KVServer.Core.Services;
using KVServer.Infrastructure.Data;
using KVServer.Infrastructure.Repositories;
using KVServer.Infrastructure.Services;

var preConfig = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables("KVSERVER_")
    .Build();

var options = ServerOptions.Parse(args, preConfig);

var builder = WebApplication.CreateBuilder(options.RemainingArgs);

// ── Logging ───────────────────────────────────────────────────────────────────

builder.Logging.SetMinimumLevel(options.LogLevel);

// ── Database path ─────────────────────────────────────────────────────────────

var dbPath = options.DbPath
    ?? Path.Combine(Directory.GetCurrentDirectory(), "data", "kvserver.db");

var dbDirectory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDirectory))
    Directory.CreateDirectory(dbDirectory);

Console.WriteLine($"Database: {dbPath}");

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddSingleton(options);
builder.Services.AddDbContext<KVServerDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<IStorageRepository, StorageRepository>();
builder.Services.AddScoped<IKeyRepository, KeyRepository>();
builder.Services.AddScoped<IVersionRepository, VersionRepository>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IKeyService, KeyService>();
builder.Services.AddControllers();

if (!options.NoCors)
{
    builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
}

// ── Port ──────────────────────────────────────────────────────────────────────

if (options.Port.HasValue || options.Bind != "localhost")
    builder.WebHost.UseUrls($"http://{options.Bind}:{options.Port ?? 5000}");

// ── Build ─────────────────────────────────────────────────────────────────────

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────

app.UseMiddleware<TokenAuthenticationMiddleware>();

if (options.ReadOnly)
    app.UseMiddleware<ReadOnlyMiddleware>();

if (!options.NoCors)
    app.UseCors("AllowAll");

if (!options.NoWeb)
    app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck");

if (!options.NoWeb)
{
    app.MapGet("/", () => Results.File("index.html", "text/html"))
       .ExcludeFromDescription();
    app.MapFallback(() => Results.File("index.html", "text/html"))
       .ExcludeFromDescription();
}

// ── Database ──────────────────────────────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<KVServerDbContext>().Database.EnsureCreated();
}

// ── Startup info ──────────────────────────────────────────────────────────────

Console.WriteLine("KVServer starting with options:");
options.PrintStartupInfo();

app.Run();
