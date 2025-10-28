using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PodcastWebApp.Data;
using PodcastWebApp.Models;
using Amazon.S3;
using Amazon.Runtime;
using DotNetEnv;

// Add detailed error handling at the top
var builder = WebApplication.CreateBuilder(args);

// Log startup information
Console.WriteLine("========================================");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
Console.WriteLine($"AWS_REGION: {Environment.GetEnvironmentVariable("AWS_REGION")}");
Console.WriteLine($"RDS_CONNECTION_STRING exists: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RDS_CONNECTION_STRING"))}");
Console.WriteLine("========================================");

// Load .env file only in development
if (builder.Environment.IsDevelopment())
{
    try
    {
        DotNetEnv.Env.Load();
        Console.WriteLine("✓ .env file loaded");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Could not load .env file: {ex.Message}");
    }
}

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure form options
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 524288000;
});

// Configure database with fallback
try
{
    if (builder.Environment.IsProduction())
    {
        var rdsConnectionString = Environment.GetEnvironmentVariable("RDS_CONNECTION_STRING");
        
        if (!string.IsNullOrEmpty(rdsConnectionString))
        {
            Console.WriteLine("✓ Using SQL Server (RDS)");
            builder.Services.AddDbContext<ApplicationDbContext>(options => 
                options.UseSqlServer(rdsConnectionString));
        }
        else
        {
            Console.WriteLine("⚠️ Using In-Memory Database (No RDS configured)");
            builder.Services.AddDbContext<ApplicationDbContext>(options => 
                options.UseInMemoryDatabase("PodcastDB"));
        }
    }
    else
    {
        Console.WriteLine("✓ Using SQLite Database (Development)");
        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=PodcastDB.db";
        builder.Services.AddDbContext<ApplicationDbContext>(options => 
            options.UseSqlite(connectionString));
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Database configuration error: {ex.Message}");
    throw;
}

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure AWS S3 Client
builder.Services.AddSingleton<IAmazonS3>(sp => 
{
    var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
    var config = new Amazon.S3.AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
    };
    
    return new Amazon.S3.AmazonS3Client(config);
});

var app = builder.Build();

// Initialize database with error handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        logger.LogInformation("=== Database Initialization ===");
        
        if (builder.Environment.IsProduction() && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RDS_CONNECTION_STRING")))
        {
            logger.LogInformation("Running migrations...");
            context.Database.Migrate();
        }
        else
        {
            logger.LogInformation("Ensuring database created...");
            context.Database.EnsureCreated();
        }
        
        // Seed roles
        string[] roles = { "Admin", "Podcaster", "Listener" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation($"✓ Created role: {role}");
            }
        }
        
        logger.LogInformation("=== Database Initialization Complete ===");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ DATABASE INITIALIZATION FAILED: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    // Don't throw - allow app to start even if DB init fails
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

Console.WriteLine("✓ Application started successfully");
app.Run();