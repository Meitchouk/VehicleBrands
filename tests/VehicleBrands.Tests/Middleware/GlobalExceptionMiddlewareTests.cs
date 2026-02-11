using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VehicleBrands.API.DTOs;
using VehicleBrands.API.Middleware;

namespace VehicleBrands.Tests.Middleware;

/// <summary>
/// Unit tests for GlobalExceptionMiddleware.
/// Verifies that unhandled exceptions are caught and returned
/// as consistent ApiResponse objects with proper status codes.
/// </summary>
public class GlobalExceptionMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _mockLogger;

    public GlobalExceptionMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldPassThrough()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new GlobalExceptionMiddleware(
            next: _ => Task.CompletedTask,
            logger: _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert — status code should remain default (200)
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionOccurs_ShouldReturn500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionMiddleware(
            next: _ => throw new InvalidOperationException("Test exception"),
            logger: _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionOccurs_ShouldReturnApiResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionMiddleware(
            next: _ => throw new Exception("Something went wrong"),
            logger: _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert — read response body and verify structure
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().NotBeNullOrWhiteSpace();
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionOccurs_ShouldNotLeakExceptionDetails()
    {
        // Arrange
        var secretMessage = "Database password is 12345";
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionMiddleware(
            next: _ => throw new Exception(secretMessage),
            logger: _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert — the secret message must NOT appear in the response
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        responseBody.Should().NotContain(secretMessage);
        responseBody.Should().Contain("unexpected error");
    }
}
