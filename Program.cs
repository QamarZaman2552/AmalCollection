using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;
using ShoppingApp.Data;
using ShoppingApp.Models;
using ShoppingApp.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

#region Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BaazWix")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/shopai-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();
#endregion

#region Services
builder.Services.AddControllersWithViews();

// Rate Limiting - Global IP-based
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    // Stricter limit for auth endpoints
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 2;
    });

    options.AddFixedWindowLimiter("CheckoutPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 5;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = 60
        }, cancellationToken: token);
    };
});

// Database
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var connectionString = builder.Configuration.GetConnectionString("dbcs")
    ?? "Host=localhost;Database=ShopAI;Username=postgres;Password=postgres;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString,
        npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null)));

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database", HealthStatus.Degraded, tags: new[] { "db" })
    .AddCheck("Self", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "self" });

// Email Settings
var emailSettings = builder.Configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);
if (!string.IsNullOrWhiteSpace(emailSettings.ApiKey))
{
    // HTTPS email API (Brevo etc.) - works on hosts that block SMTP (Railway free/trial)
    builder.Services.AddHttpClient<IEmailSender, HttpEmailSender>();
}
else if (emailSettings.Enabled && !string.IsNullOrWhiteSpace(emailSettings.Host))
{
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
}
else
{
    builder.Services.AddScoped<IEmailSender, DevEmailSender>();
}

// Application Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<RecommendationService>();

// Gemini Chatbot Service
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.AddHttpClient<IGeminiChatService, GeminiChatService>();
builder.Services.AddScoped<SiteContextService>();

// Session - Environment-aware
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    
    if (builder.Environment.IsDevelopment())
    {
        // Development: Allow HTTP, Lax SameSite
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "Session";
    }
    else
    {
        // Production: HTTPS only, Strict SameSite, __Host- prefix
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = "__Host-Session";
    }
});

// Antiforgery - Environment-aware
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    
    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.Name = "XSRF-TOKEN";
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.Cookie.SameSite = SameSiteMode.Lax;
    }
    else
    {
        options.Cookie.Name = "__Host-XSRF-TOKEN";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    }
    
    options.Cookie.HttpOnly = true;
    options.SuppressXFrameOptionsHeader = false;
});

// Security Headers via custom middleware (added in pipeline)
#endregion

var app = builder.Build();

// Enable request buffering so body can be read multiple times
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

#region Pipeline - Security First
// Trust proxy headers (Render/Railway terminate TLS at their edge)
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
        | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
// Trust all proxies: Railway/Render edge IPs are dynamic
forwardedOptions.KnownIPNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

// Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null ? Serilog.Events.LogEventLevel.Error :
        httpContext.Response.StatusCode > 499 ? Serilog.Events.LogEventLevel.Warning :
        Serilog.Events.LogEventLevel.Information;
});

// HTTPS Enforcement
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Security Headers Middleware (environment-aware)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
    context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";

    // CSP: Different for Development vs Production
    if (app.Environment.IsDevelopment())
    {
        // Development: Allow VS Browser Link, Hot Reload, WebSockets
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://fonts.googleapis.com; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com data:; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self' http://localhost:* https://localhost:* ws://localhost:* wss://localhost:*; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self';";
    }
    else
    {
        // Production: Strict
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com data:; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self';";
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static assets for 1 year
        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
    }
});

app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAntiforgery();
app.UseAuthorization();

#endregion

#region Health Check Endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapGet("/diag-email", async (EmailSettings s, IEmailSender sender) =>
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"Enabled={s.Enabled}; Host={s.Host}; Port={s.Port}; Ssl={s.EnableSsl}; User={s.UserName}; From={s.FromEmail}; FromName={s.FromName}; PwdLen={(s.Password ?? "").Length}; ApiKeyLen={(s.ApiKey ?? "").Length}; AdminEmail={s.AdminEmail}");
    sb.AppendLine($"SenderType={sender.GetType().Name}");
    try
    {
        await sender.SendAsync("qamarbaloch2023@gmail.com", "BaazWix Live SMTP Test", "<p>Test from Railway</p>");
        sb.AppendLine("SEND_OK");
    }
    catch (Exception ex)
    {
        sb.AppendLine($"SEND_FAIL: {ex.GetType().Name}: {ex.Message}");
        if (ex.InnerException != null) sb.AppendLine($"INNER: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
    }
    return Results.Text(sb.ToString());
});
#endregion

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

#region Startup Logging (Auto-Migrate on Production)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (app.Environment.IsProduction())
        {
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrated and seeded");
        }
        var canConnect = await db.Database.CanConnectAsync();
        logger.LogInformation("Database connectivity: {Status}", canConnect ? "OK" : "FAILED");
        logger.LogInformation("Application starting in {Environment} mode", app.Environment.EnvironmentName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Startup database check failed");
    }
}
#endregion

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}