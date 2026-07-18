using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();

// Always use appsettings connection (localhost) so cart, checkout, and admin share one database.
var selectedCs = builder.Configuration.GetConnectionString("dbcs")
    ?? "Server=localhost;Database=ShopAI;Trusted_Connection=True;TrustServerCertificate=True;";
var selectedName = "Localhost";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(selectedCs));


builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<RecommendationService>();

var emailSettings = builder.Configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);
if (emailSettings.Enabled && !string.IsNullOrWhiteSpace(emailSettings.Host))
{
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
}
else
{
    builder.Services.AddScoped<IEmailSender, DevEmailSender>();
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Log selected connection choice for developer visibility
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Database connection probe selected provider: {Provider}", selectedName);
    }
    catch
    {
        // ignore logging failures
    }
}

// Auto-migrate and seed on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log the error but allow the app to continue starting so the developer can inspect details
        logger.LogError(ex, "Database migration failed on startup. Server will continue to run.");
    }
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Development-only health endpoint to inspect DB connectivity and selected provider
if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev-db-health", async (IServiceProvider sp) =>
    {
        using var scope = sp.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        object resultObj = new { provider = selectedName, canConnect = false, dbExists = false, message = "" };
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var canConnect = await db.Database.CanConnectAsync();
            bool dbExists = false;
            try
            {
                var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT DB_ID('ShopAI')";
                var val = await cmd.ExecuteScalarAsync();
                dbExists = val != null && val != DBNull.Value;
                await conn.CloseAsync();
            }
            catch { }

            resultObj = new { provider = selectedName, canConnect = canConnect, dbExists = dbExists, message = canConnect ? "connected" : "cannot connect" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dev DB health check failed");
            resultObj = new { provider = selectedName, canConnect = false, dbExists = false, message = ex.Message };
        }

        return Results.Json(resultObj);
    });
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
