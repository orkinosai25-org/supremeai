namespace SupremeAI.Api.Middleware;

/// <summary>
/// API governance middleware that runs on every request.
///
/// Responsibilities:
///   - Attaches a unique <c>X-Request-Id</c> to every response so that calls
///     can be correlated across logs and downstream systems.
///   - Stamps <c>X-Api-Version</c> so clients can confirm which API release
///     they are talking to.
///   - Records wall-clock latency for every request/response cycle and emits
///     structured log entries (inbound + outbound) for audit purposes.
/// </summary>
public sealed class GovernanceMiddleware
{
    internal const string ApiVersion = "v0.3.2-api-governance";

    private readonly RequestDelegate _next;
    private readonly ILogger<GovernanceMiddleware> _logger;

    public GovernanceMiddleware(RequestDelegate next, ILogger<GovernanceMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString("N")[..16];
        var sw        = System.Diagnostics.Stopwatch.StartNew();

        // Attach governance headers before the downstream handler runs so that
        // they are always present even when the response is written directly
        // (e.g. by minimal-API endpoints or the rate-limiter rejection handler).
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Request-Id"]  = requestId;
            context.Response.Headers["X-Api-Version"] = ApiVersion;
            return Task.CompletedTask;
        });

        _logger.LogInformation(
            "→ {Method} {Path} [{RequestId}]",
            context.Request.Method,
            context.Request.Path,
            requestId);

        await _next(context);

        sw.Stop();
        _logger.LogInformation(
            "← {StatusCode} {Method} {Path} [{RequestId}] {ElapsedMs}ms",
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path,
            requestId,
            sw.ElapsedMilliseconds);
    }
}
