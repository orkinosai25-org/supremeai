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
        // Use the full GUID (32 hex chars) to guarantee uniqueness in audit logs.
        var requestId = Guid.NewGuid().ToString("N");
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

        // Sanitise user-supplied values before writing them to the log to
        // prevent log-forging attacks (CWE-117 / CodeQL cs/log-forging).
        var method = Sanitize(context.Request.Method);
        var path   = Sanitize(context.Request.Path.Value ?? "");

        _logger.LogInformation(
            "→ {Method} {Path} [{RequestId}]",
            method,
            path,
            requestId);

        await _next(context);

        sw.Stop();
        _logger.LogInformation(
            "← {StatusCode} {Method} {Path} [{RequestId}] {ElapsedMs}ms",
            context.Response.StatusCode,
            method,
            path,
            requestId,
            sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Removes all ASCII control characters (0x00–0x1F and 0x7F) from a string
    /// that originates from the HTTP request to prevent log-forging (CWE-117).
    /// </summary>
    private static string Sanitize(string value) =>
        new string(value.Select(c => char.IsControl(c) ? ' ' : c).ToArray());
}
