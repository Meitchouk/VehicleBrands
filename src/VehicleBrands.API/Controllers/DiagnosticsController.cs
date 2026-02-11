using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace VehicleBrands.API.Controllers;

/// <summary>
/// Diagnostic and monitoring endpoints for API health and information
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public DiagnosticsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Get API information including version, environment, and uptime
    /// </summary>
    /// <returns>API metadata and status information</returns>
    /// <response code="200">Returns API information</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        var uptime = DateTime.UtcNow - _startTime;

        var info = new
        {
            name = "Vehicle Brands API",
            version,
            environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production",
            uptime = new
            {
                days = uptime.Days,
                hours = uptime.Hours,
                minutes = uptime.Minutes,
                seconds = uptime.Seconds,
                totalSeconds = (int)uptime.TotalSeconds
            },
            timestamp = DateTime.UtcNow,
            dotnetVersion = Environment.Version.ToString(),
            machineName = Environment.MachineName,
            osVersion = Environment.OSVersion.ToString(),
            processorCount = Environment.ProcessorCount
        };

        return Ok(info);
    }

    /// <summary>
    /// Get API version information
    /// </summary>
    /// <returns>API version details</returns>
    /// <response code="200">Returns version information</response>
    [HttpGet("version")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? version;

        var versionInfo = new
        {
            version,
            informationalVersion,
            apiVersion = "v1",
            buildDate = new FileInfo(assembly.Location).LastWriteTimeUtc
        };

        return Ok(versionInfo);
    }

    /// <summary>
    /// Get current server time in UTC
    /// </summary>
    /// <returns>Current UTC timestamp</returns>
    /// <response code="200">Returns current server time</response>
    [HttpGet("time")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetTime()
    {
        var timeInfo = new
        {
            utc = DateTime.UtcNow,
            local = DateTime.Now,
            timezone = TimeZoneInfo.Local.DisplayName,
            unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        return Ok(timeInfo);
    }

    /// <summary>
    /// Get API status summary
    /// </summary>
    /// <returns>Quick status check</returns>
    /// <response code="200">API is operational</response>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        var uptime = DateTime.UtcNow - _startTime;

        var status = new
        {
            status = "healthy",
            message = "API is operational",
            uptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s",
            timestamp = DateTime.UtcNow
        };

        return Ok(status);
    }
}
