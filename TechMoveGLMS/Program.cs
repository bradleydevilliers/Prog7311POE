using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Services.Contracts;
using TechMoveGLMS.Services.Notifications;
using TechMoveGLMS.Services.Pricing;
using TechMoveGLMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register HttpClient for Currency API
builder.Services.AddHttpClient<CurrencyService>();

// Register Factory Method Pattern
builder.Services.AddScoped<IContractFactory, ContractFactory>();
// Register Observer Pattern services
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddScoped<INotificationObserver, EmailNotifier>();
builder.Services.AddScoped<INotificationObserver, ComplianceLogger>();
  
// Register Strategy Pattern services
builder.Services.AddScoped<PricingContext>();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
// AUTO-CREATE DATABASE AND TABLES ON STARTUP
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
    Console.WriteLine(" Database and tables created/verified successfully!");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
