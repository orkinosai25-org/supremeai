using System.Threading.RateLimiting;
using Azure.Identity;
using Microsoft.AspNetCore.RateLimiting;
using SupremeAI.Api.Middleware;
using SupremeAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Load .env file (if present) ───────────────────────────────────────────────
// Supports running with `dotnet run` without setting environment variables.
// The .env file is the same directory as the project (src/SupremeAI.Api/.env).
var envFile = Path.Combine(AppContext.BaseDirectory, ".env");
// Also try the project source directory when running in development
var envFileDev = Path.Combine(Directory.GetCurrentDirectory(), ".env");
foreach (var ef in new[] { envFile, envFileDev })
{
    if (File.Exists(ef))
    {
        foreach (var line in File.ReadAllLines(ef))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
            var eq = trimmed.IndexOf('=');
            if (eq < 0) continue;
            var key = trimmed[..eq].Trim();
            var val = trimmed[(eq + 1)..].Trim();
            // Only set if not already in environment (env vars take precedence)
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, val);
        }
        Console.WriteLine($"[INFO] Loaded environment from {ef}");
        break;
    }
}

// ── Key Vault integration (optional) ─────────────────────────────────────────
// If AZURE_KEYVAULT_URI or AzureKeyVaultUri is configured, load all secrets
// into the configuration system so providers can read them via IConfiguration.
var keyVaultUri = builder.Configuration["AZURE_KEYVAULT_URI"]
               ?? builder.Configuration["AzureKeyVaultUri"];

if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    try
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());

        Console.WriteLine($"[INFO] Azure Key Vault configured: {keyVaultUri}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] Could not load Azure Key Vault '{keyVaultUri}': {ex.Message}");
    }
}

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title   = "SupremeAI API",
        Version = GovernanceMiddleware.ApiVersion,
        Description =
            "**SupremeAI** is a judgment and assurance layer that evaluates multiple AI models, " +
            "estimates confidence, and provides explainable decisions. " +
            "It does not claim objective truth or replace human judgment.\n\n" +
            "> **Disclaimer:** SupremeAI provides judgment, confidence, and comparative evaluation; " +
            "it does not assert objective truth or replace human decision‑making.\n\n" +
            "## API Groups\n\n" +
            "| Group | Purpose |\n" +
            "|---|---|\n" +
            "| **API Governance** | Health, liveness, and version endpoints. " +
            "Use `GET /health` as a liveness/readiness probe and `GET /version` to confirm the API release. |\n" +
            "| **SupremeAI — Judgment & Governance** | Primary endpoints. Run the Judgment Engine, inspect model performance profiles, and execute benchmarks. These are the recommended endpoints for production and public-sector deployments. |\n" +
            "| **SupremeAI — Primary Endpoint** | Unified frontend endpoint. Fans the prompt across all selected models via the Judgment Engine and returns the winning answer together with confidence score and rationale. This is the default endpoint used by the SupremeAI frontend. |\n" +
            "| **Legacy — Direct Access** | Direct AI generation. Bypasses SupremeAI judgment and confidence mechanisms. Not recommended for production or public-sector use. |",
    });

    // Route each action to the correct Swagger tag so that governance
    // endpoints appear first and legacy direct-access endpoints are
    // clearly labelled as secondary / not recommended for production use.
    c.TagActionsBy(api =>
    {
        var controller = api.ActionDescriptor.RouteValues["controller"];
        var action     = api.ActionDescriptor.RouteValues["action"];

        return (controller, action) switch
        {
            ("Ai", "Supreme")  => ["SupremeAI — Primary Endpoint"],
            ("Ai", _)          => ["Legacy — Direct Access"],
            ("Governance", _)  => ["API Governance"],
            _                  => ["SupremeAI — Judgment & Governance"],
        };
    });
});

// Named HttpClients for each external provider
builder.Services.AddHttpClient("anthropic")
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(120));

builder.Services.AddHttpClient("google")
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(120));

builder.Services.AddHttpClient("xai")
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(120));

// Register model providers (order matters for CanHandle resolution)
builder.Services.AddSingleton<IModelProvider, AzureOpenAiProvider>();
builder.Services.AddSingleton<IModelProvider, AzureAiInferenceProvider>();
builder.Services.AddScoped<IModelProvider, AnthropicProvider>();
builder.Services.AddScoped<IModelProvider, GoogleProvider>();
builder.Services.AddScoped<IModelProvider, XaiProvider>();

builder.Services.AddScoped<ModelProviderFactory>();
builder.Services.AddScoped<BrainService>();

// ── Judgment Engine ──────────────────────────────────────────────────────────
builder.Services.AddSingleton<JudgmentStore>();
builder.Services.AddScoped<JudgmentEngine>();
builder.Services.AddScoped<JudgmentAnalyticsService>();

// ── Benchmark & Publishing Layer ──────────────────────────────────────────────
builder.Services.AddSingleton<BenchmarkStore>();
builder.Services.AddScoped<BenchmarkService>();

// ── Rate Limiting (API governance) ───────────────────────────────────────────
// Global limit:  100 requests per 60 s per remote IP (all endpoints).
// "ai-strict":    20 requests per 60 s per remote IP (expensive AI endpoints).
builder.Services.AddRateLimiter(options =>
{
    // Global fallback: applied to every endpoint not covered by a named policy.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit          = 100,
            Window               = TimeSpan.FromSeconds(60),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit           = 0,
        });
    });

    // Named policy for AI-generation endpoints (POST /api/ai/*).
    options.AddPolicy("ai-strict", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit          = 20,
            Window               = TimeSpan.FromSeconds(60),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit           = 0,
        });
    });

    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.StatusCode  = StatusCodes.Status429TooManyRequests;
        ctx.HttpContext.Response.Headers["Retry-After"] = "60";
        await ctx.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Rate limit exceeded. Please retry after 60 seconds." }, ct);
    };
});

// ── CORS – allow the Blazor WASM frontend ─────────────────────────────────────
var frontendOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:5095", "https://localhost:7186"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorFrontend", policy =>
    {
        policy
            .WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── Build & configure pipeline ─────────────────────────────────────────────

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseBlazorFrameworkFiles();

app.UseCors("BlazorFrontend");
app.UseRateLimiter();
app.UseMiddleware<GovernanceMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Blazor UI is the default for "/"
app.MapFallbackToFile("index.html");

app.Run();
