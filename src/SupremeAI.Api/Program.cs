using Azure.Identity;
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
    c.SwaggerDoc("v1", new() { Title = "SupremeAI API", Version = "v1" });
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BlazorFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
