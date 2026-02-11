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

// Apply pending migrations and seed database.
// Migrate() is idempotent — it only applies migrations
// not yet recorded in __EFMigrationsHistory.
// DatabaseSeeder is intelligent — it only adds brands
// that don't already exist (checks by name).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Apply pending migrations
    dbContext.Database.Migrate();

    // Seed database with 50+ car brands (only if they don't exist)
    await DatabaseSeeder.SeedAsync(dbContext, logger);
}

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
