using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using KVServer.Api.Middleware;
using KVServer.Core.Repositories;
using KVServer.Core.Services;
using KVServer.Infrastructure.Data;
using KVServer.Infrastructure.Repositories;
using KVServer.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Get database path from configuration
var dbPath = builder.Configuration["DbPath"];
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

Console.WriteLine($"Database path: {dbPath}");

// Add DbContext
builder.Services.AddDbContext<KVServerDbContext>(options =>
    options.UseSqlite(connectionString));

// Register services
builder.Services.AddScoped<IStorageRepository, StorageRepository>();
builder.Services.AddScoped<IKeyRepository, KeyRepository>();
builder.Services.AddScoped<IVersionRepository, VersionRepository>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IKeyService, KeyService>();

// Add services to the container.
builder.Services.AddControllers();

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use authentication middleware before routing
app.UseMiddleware<TokenAuthenticationMiddleware>();

app.UseCors("AllowAll");
app.UseStaticFiles(); // Serve static files from wwwroot
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .RequireHost("*");

// Serve index.html as default route for SPA
app.MapGet("/", () => Results.File("index.html", "text/html"))
   .ExcludeFromDescription();

// API fallback - return 404 for unknown API routes
app.MapFallback(() => Results.File("index.html", "text/html"))
   .ExcludeFromDescription();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KVServerDbContext>();
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Database created/verified successfully");
}

app.Run();
