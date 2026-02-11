using System.Reflection;
using Asp.Versioning;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using VehicleBrands.API.Middleware;
using VehicleBrands.Infrastructure;
using VehicleBrands.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Register services in the DI container

// Infrastructure layer services (DbContext + Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// MVC Controllers
builder.Services.AddControllers();

// API Versioning — URL segment strategy (e.g. /api/v1/MarcasAutos)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger/OpenAPI with XML documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Vehicle Brands API",
        Version = "v1",
        Description = "REST API for managing car brands"
    });

    // Include XML comments from the API assembly for Swagger documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Health Checks — validates database connectivity
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: ["db", "ready"]);

var app = builder.Build();

// Apply pending migrations and seed database with retry logic for cloud deployments.
// Railway/cloud platforms may need a few seconds for the database to become available.
await ApplyMigrationsAndSeedAsync(app.Services);

// HTTP middleware pipeline

// Global error handling — must be first to catch all exceptions
app.UseGlobalExceptionHandling();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vehicle Brands API v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health check endpoints for monitoring and orchestration
// Liveness probe - checks if the application process is running
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // Don't run any checks, just verify the process is alive
    AllowCachingResponses = false
});

// Readiness probe - checks if the application is ready to serve requests (DB connectivity)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    AllowCachingResponses = false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Detailed health check with full JSON response
app.MapHealthChecks("/health", new HealthCheckOptions
{
    AllowCachingResponses = false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

// Apply EF Core migrations and seed the database with retry logic.
// Railway and other cloud platforms may need time for the database to become available.
static async Task ApplyMigrationsAndSeedAsync(IServiceProvider services)
{
    const int maxRetries = 10;
    const int initialDelayMs = 1000;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Attempting to connect to database... (Attempt {Attempt}/{MaxRetries})", attempt, maxRetries);

            // Test database connectivity
            await dbContext.Database.CanConnectAsync();
            logger.LogInformation("Database connection successful.");

            // Apply pending migrations (idempotent - only applies new migrations)
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");

            // Seed database with car brands (intelligent - checks for existing data)
            logger.LogInformation("Seeding database...");
            await DatabaseSeeder.SeedAsync(dbContext, logger);
            logger.LogInformation("Database seeding completed.");

            return; // Success - exit retry loop
        }
        catch (Exception ex)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            if (attempt == maxRetries)
            {
                logger.LogCritical(ex, "Failed to initialize database after {MaxRetries} attempts. Application will start but may not function correctly.", maxRetries);
                return; // Give up after max retries - let app start anyway for health checks to work
            }

            // Exponential backoff: 1s, 2s, 4s, 8s, 16s, 32s...
            var delayMs = initialDelayMs * (int)Math.Pow(2, attempt - 1);
            delayMs = Math.Min(delayMs, 30000); // Cap at 30 seconds

            logger.LogWarning(ex, "Database connection failed on attempt {Attempt}/{MaxRetries}. Retrying in {DelaySeconds} seconds...", 
                attempt, maxRetries, delayMs / 1000);

            await Task.Delay(delayMs);
        }
    }
}
