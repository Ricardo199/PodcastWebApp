using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PodcastWebApp.Data;
using PodcastWebApp.Models;
using Amazon.S3;
using Amazon.Runtime;
using DotNetEnv;

// FIRST: Log environment immediately
Console.WriteLine($"========================================");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
Console.WriteLine($"========================================");

// Load .env file only in development
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    Env.Load();
}

// Create web application builder
var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Builder Environment Name: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Is Production: {builder.Environment.IsProduction()}");
Console.WriteLine($"Is Development: {builder.Environment.IsDevelopment()}");

// Add MVC controllers and views
builder.Services.AddControllersWithViews();
// Add Razor Pages support
builder.Services.AddRazorPages();

// Configure database - Use In-Memory for production (no file permissions issues)
if (builder.Environment.IsProduction())
{
    Console.WriteLine("✓ Using In-Memory Database (Production)");
    builder.Services.AddDbContext<ApplicationDbContext>(options => 
        options.UseInMemoryDatabase("PodcastDB"));
}
else
{
    Console.WriteLine("✓ Using SQLite Database (Development)");
    // Use SQLite for development
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=PodcastDB.db";
    builder.Services.AddDbContext<ApplicationDbContext>(options => 
        options.UseSqlite(connectionString));
}

// Add user authentication
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure AWS S3 Client - supports both explicit credentials and IAM roles
builder.Services.AddSingleton<IAmazonS3>(sp => 
{
    var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
    var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
    var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
    
    var config = new Amazon.S3.AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
    };
    
    // Use explicit credentials if provided, otherwise fallback to IAM role/instance profile
    if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
    {
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        return new Amazon.S3.AmazonS3Client(credentials, config);
    }
    else
    {
        // Use default credential chain (IAM role, environment variables, etc.)
        return new Amazon.S3.AmazonS3Client(config);
    }
});

// Build the application
var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        logger.LogInformation("=== Database Initialization ===");
        logger.LogInformation($"Environment: {builder.Environment.EnvironmentName}");
        logger.LogInformation($"Database Type: {(builder.Environment.IsProduction() ? "In-Memory" : "SQLite")}");
        
        // Ensure database is created
        logger.LogInformation("Creating database...");
        context.Database.EnsureCreated();
        logger.LogInformation("✓ Database created successfully");
        
        // Seed roles
        logger.LogInformation("Seeding roles...");
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
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ DATABASE INITIALIZATION FAILED");
        logger.LogError($"Error: {ex.Message}");
    }
}

// Configure error handling for production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Use HTTPS
app.UseHttpsRedirection();
// Serve static files (CSS, JS, images)
app.UseStaticFiles();
// Enable routing
app.UseRouting();
// Enable user authentication
app.UseAuthentication();
// Enable user authorization
app.UseAuthorization();
// Set default route pattern
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
// Map Razor Pages
app.MapRazorPages();

// Start the application
app.Run();