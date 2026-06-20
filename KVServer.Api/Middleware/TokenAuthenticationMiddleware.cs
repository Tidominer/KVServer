using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using KVServer.Core.Models;
using KVServer.Core.Services;
using KVServer.Infrastructure.Data;

namespace KVServer.Api.Middleware;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;
    private readonly ServerOptions _serverOptions;
    private static readonly ConcurrentDictionary<string, FailedLoginAttempts> _failedAttempts = new();
    private static readonly Timer _cleanupTimer;

    static TokenAuthenticationMiddleware()
    {
        _cleanupTimer = new Timer(_ => CleanupOldEntries(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public TokenAuthenticationMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, ILogger<TokenAuthenticationMiddleware> logger, ServerOptions serverOptions)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _serverOptions = serverOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only authenticate API routes
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KVServerDbContext>();
            var clientIp = GetClientIpAddress(context);

            if (!context.Request.Headers.TryGetValue("X-Access-Token", out var tokenHeader))
            {
                _logger.LogWarning("Failed login attempt: Missing access token from IP: {ClientIP}", clientIp);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Missing access token" });
                return;
            }

            var token = tokenHeader.ToString();
            var storage = await dbContext.Storages
                .FirstOrDefaultAsync(s => s.AccessToken == token && s.IsActive);

            if (storage == null)
            {
                // Check rate limiting
                if (IsRateLimited(clientIp, out var retryAfter))
                {
                    _logger.LogWarning("Rate limit exceeded for IP: {ClientIP}. Retry after: {RetryAfterSeconds} seconds", clientIp, retryAfter);
                    context.Response.StatusCode = 429;
                    context.Response.Headers["Retry-After"] = retryAfter.ToString();
                    await context.Response.WriteAsJsonAsync(new {
                        error = "Too many failed login attempts. Please try again later.",
                        retryAfter = $"{retryAfter} seconds"
                    });
                    return;
                }

                // Record failed attempt
                RecordFailedAttempt(clientIp);
                _logger.LogWarning("Failed login attempt: Invalid access token '{TokenTruncated}' from IP: {ClientIP}", TruncateToken(token), clientIp);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid access token" });
                return;
            }

            _logger.LogInformation("Successful login: Storage: '{StorageName}', Token: '{TokenTruncated}', IP: {ClientIP}", storage.Name, TruncateToken(token), clientIp);

            // Clear failed attempts on successful login
            ClearFailedAttempts(clientIp);

            // Add storage to context items for use in controllers
            context.Items["Storage"] = storage;
        }

        await _next(context);
    }

    private bool IsRateLimited(string clientIp, out int retryAfter)
    {
        retryAfter = 0;

        if (!_failedAttempts.TryGetValue(clientIp, out var attempts))
        {
            return false;
        }

        var now = DateTime.UtcNow;
        attempts.RemoveOldAttempts(now.Subtract(TimeSpan.FromMinutes(1)));

        if (attempts.RecentAttempts.Count >= _serverOptions.RateLimit)
        {
            var oldestAttempt = attempts.RecentAttempts[0];
            retryAfter = 60 - (int)(now - oldestAttempt).TotalSeconds;
            if (retryAfter < 0) retryAfter = 0;
            return true;
        }

        return false;
    }

    private static void RecordFailedAttempt(string clientIp)
    {
        var attempts = _failedAttempts.GetOrAdd(clientIp, _ => new FailedLoginAttempts());
        attempts.AddAttempt(DateTime.UtcNow);
    }

    private static void ClearFailedAttempts(string clientIp)
    {
        _failedAttempts.TryRemove(clientIp, out _);
    }

    private static void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddMinutes(-10); // Remove entries older than 10 minutes

        foreach (var key in _failedAttempts.Keys)
        {
            if (_failedAttempts.TryGetValue(key, out var attempts))
            {
                attempts.RemoveOldAttempts(cutoff);

                if (attempts.RecentAttempts.Count == 0)
                {
                    _failedAttempts.TryRemove(key, out _);
                }
            }
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for proxy headers first
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor.ToString().Split(',')[0].Trim();
        }

        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            return realIp.ToString();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static string TruncateToken(string token, int maxLength = 8)
    {
        if (string.IsNullOrEmpty(token)) return "Empty";
        return token.Length > maxLength ? $"{token.Substring(0, maxLength)}..." : token;
    }

    private class FailedLoginAttempts
    {
        public List<DateTime> RecentAttempts { get; } = new();

        public void AddAttempt(DateTime attempt)
        {
            RecentAttempts.Add(attempt);
        }

        public void RemoveOldAttempts(DateTime cutoff)
        {
            RecentAttempts.RemoveAll(attempt => attempt < cutoff);
        }
    }
}