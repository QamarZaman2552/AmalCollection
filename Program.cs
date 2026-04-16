using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<RecommendationService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Auto-migrate and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
//migrationBuilder.UpdateData(
//    table: "Users",
//    keyColumn: "Id",
//    keyValue: 1,
//    column: "PasswordHash",
//    value: "$2a$11$mQC6j0QRHRTWlwB9gGmyiurcaBBw1XDLhD8.8Oe0jNp5NGfcLnXAG");
