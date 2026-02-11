using FluentAssertions;
using VehicleBrands.API.DTOs;
using Xunit;

namespace VehicleBrands.Tests.DTOs;

public class ApiResponseTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessResponse()
    {
        // Arrange
        var data = "Test data";

        // Act
        var result = ApiResponse<string>.Ok(data);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().Be(data);
        result.Success.Should().BeTrue();
        result.Message.Should().BeNull();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Ok_WithMessage_ShouldCreateSuccessResponseWithMessage()
    {
        // Arrange
        var data = "Test data";
        var message = "Operation successful";

        // Act
        var result = ApiResponse<string>.Ok(data, message);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().Be(data);
        result.Success.Should().BeTrue();
        result.Message.Should().Be(message);
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundResponse()
    {
        // Arrange
        var message = "Resource not found";

        // Act
        var result = ApiResponse<string>.NotFound(message);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
    }

    [Fact]
    public void ValidationError_ShouldCreateValidationErrorResponse()
    {
        // Arrange
        var message = "Validation failed";
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1", "Error 2" } },
            { "Field2", new[] { "Error 3" } }
        };

        // Act
        var result = ApiResponse<string>.ValidationError(message, errors);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().ContainKey("Field1");
        result.Errors.Should().ContainKey("Field2");
    }

    [Fact]
    public void Conflict_ShouldCreateConflictResponse()
    {
        // Arrange
        var message = "Resource already exists";

        // Act
        var result = ApiResponse<string>.Conflict(message);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
    }
}

public class ApiListResponseTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessListResponse()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };

        // Act
        var result = ApiListResponse<string>.Ok(items);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.Data.Should().BeEquivalentTo(items);
        result.Count.Should().Be(3);
        result.Success.Should().BeTrue();
        result.Message.Should().BeNull();
    }

    [Fact]
    public void Ok_WithMessage_ShouldIncludeMessage()
    {
        // Arrange
        var items = new List<string> { "Item1" };
        var message = "Retrieved successfully";

        // Act
        var result = ApiListResponse<string>.Ok(items, message);

        // Assert
        result.Message.Should().Be(message);
        result.Count.Should().Be(1);
    }

    [Fact]
    public void OkPaged_ShouldCreatePagedResponse()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var totalCount = 10;
        var page = 1;
        var pageSize = 3;

        // Act
        var result = ApiListResponse<string>.OkPaged(items, totalCount, page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.Count.Should().Be(3);
        result.TotalCount.Should().Be(totalCount);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void Ok_EmptyList_ShouldReturnValidResponse()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var result = ApiListResponse<string>.Ok(items);

        // Assert
        result.Data.Should().BeEmpty();
        result.Count.Should().Be(0);
        result.Success.Should().BeTrue();
    }
}
