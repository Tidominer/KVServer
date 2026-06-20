namespace KVServer.Api.Middleware;

public class ReadOnlyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ServerOptions _options;

    public ReadOnlyMiddleware(RequestDelegate next, ServerOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_options.ReadOnly &&
            context.Request.Path.StartsWithSegments("/api/keys") &&
            context.Request.Method is "POST" or "PUT" or "DELETE" or "PATCH")
        {
            context.Response.StatusCode = 405;
            await context.Response.WriteAsJsonAsync(new { error = "Server is running in read-only mode." });
            return;
        }

        await _next(context);
    }
}
