using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VehicleBrands.API.Controllers;
using VehicleBrands.API.DTOs;
using VehicleBrands.API.Mappings;
using VehicleBrands.Domain.Entities;
using VehicleBrands.Domain.Interfaces;

namespace VehicleBrands.Tests.Controllers;

/// <summary>
/// Unit tests for MarcasAutosController.
/// Uses mocks to isolate the controller from real dependencies,
/// verifying only the controller's own logic.
/// </summary>
public class MarcasAutosControllerTests
{
    private readonly Mock<ICarBrandRepository> _mockRepository;
    private readonly Mock<ILogger<MarcasAutosController>> _mockLogger;
    private readonly MarcasAutosController _controller;

    public MarcasAutosControllerTests()
    {
        _mockRepository = new Mock<ICarBrandRepository>();
        _mockLogger = new Mock<ILogger<MarcasAutosController>>();
        _controller = new MarcasAutosController(_mockRepository.Object, _mockLogger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // GetAll tests

    [Fact]
    public async Task GetAll_ShouldReturnOkResult_WithListOfBrands()
    {
        // Arrange
        var brands = GetTestBrands();
        _mockRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CarBrandQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((brands, brands.Count));

        // Act
        var result = await _controller.GetAll(cancellationToken: CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiListResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Count.Should().Be(3);
        response.TotalCount.Should().Be(3);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithEmptyList_WhenNoBrandsExist()
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CarBrandQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<CarBrand>(), 0));

        // Act
        var result = await _controller.GetAll(cancellationToken: CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiListResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Count.Should().Be(0);
        response.TotalCount.Should().Be(0);
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnCorrectDtoMapping()
    {
        // Arrange
        var brands = GetTestBrands();
        _mockRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CarBrandQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((brands, brands.Count));

        // Act
        var result = await _controller.GetAll(cancellationToken: CancellationToken.None);

        // Assert - Verify Entity -> DTO mapping is correct
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiListResponse<CarBrandDto>>().Subject;
        var returnedBrands = response.Data!.ToList();

        returnedBrands[0].Name.Should().Be("Toyota");
        returnedBrands[0].CountryOfOrigin.Should().Be("Japan");
        returnedBrands[0].FoundedYear.Should().Be(1937);
        returnedBrands[0].Website.Should().Be("https://www.toyota.com");
        returnedBrands[0].IsLuxury.Should().BeFalse();
        returnedBrands[0].Headquarters.Should().Be("Toyota City, Aichi");
        returnedBrands[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_ShouldCallRepositoryOnce()
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CarBrandQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetTestBrands(), 3));

        // Act
        await _controller.GetAll(cancellationToken: CancellationToken.None);

        // Assert - Verify the repository was called exactly once
        _mockRepository.Verify(
            repo => repo.GetAllAsync(It.IsAny<CarBrandQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithInvalidPaging_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetAll(
            name: null,
            country: null,
            isLuxury: null,
            includeInactive: false,
            page: 0,
            pageSize: 100,
            cancellationToken: CancellationToken.None);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().ContainKey("page");
        response.Errors.Should().ContainKey("pageSize");
    }

    [Fact]
    public async Task GetAll_WithIncludeInactive_ShouldPassQueryToRepository()
    {
        // Arrange
        var brands = GetTestBrands();
        _mockRepository
            .Setup(repo => repo.GetAllAsync(
                It.Is<CarBrandQuery>(q => q.IncludeInactive),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((brands, brands.Count));

        // Act
        var result = await _controller.GetAll(
            name: null,
            country: null,
            isLuxury: null,
            includeInactive: true,
            page: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockRepository.Verify(
            repo => repo.GetAllAsync(It.Is<CarBrandQuery>(q => q.IncludeInactive), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // GetById tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var brand = GetTestBrands().First();
        _mockRepository
            .Setup(repo => repo.GetByIdAsync(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Id.Should().Be(1);
        response.Data.Name.Should().Be("Toyota");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.GetByIdAsync(999, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CarBrand?)null);

        // Act
        var result = await _controller.GetById(999, CancellationToken.None);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = notFoundResult.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("999");
    }

    [Fact]
    public async Task GetById_ShouldReturnCorrectDtoMapping()
    {
        // Arrange
        var brand = new CarBrand
        {
            Id = 2,
            Name = "Ford",
            CountryOfOrigin = "United States",
            FoundedYear = 1903,
            Website = "https://www.ford.com",
            IsLuxury = false,
            Headquarters = "Dearborn, Michigan"
        };

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brand);

        // Act
        var result = await _controller.GetById(2, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        var dto = response.Data!;
        dto.Id.Should().Be(2);
        dto.Name.Should().Be("Ford");
        dto.CountryOfOrigin.Should().Be("United States");
        dto.FoundedYear.Should().Be(1903);
        dto.Website.Should().Be("https://www.ford.com");
        dto.IsLuxury.Should().BeFalse();
        dto.Headquarters.Should().Be("Dearborn, Michigan");
    }

    // Create tests

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        var created = request.ToEntity();
        created.Id = 10;

        _mockRepository
            .Setup(repo => repo.ExistsByNameAsync(request.Name, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<CarBrand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Id.Should().Be(10);
        response.Data.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task Create_WithInvalidModel_ShouldReturnBadRequest()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task Create_WithDuplicateName_ShouldReturnConflict()
    {
        // Arrange
        var request = CreateValidCreateRequest();
        _mockRepository
            .Setup(repo => repo.ExistsByNameAsync(request.Name, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var conflict = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var response = conflict.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("already exists");
    }

    // Update tests

    [Fact]
    public async Task Update_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        var updated = request.ToEntity(1);

        _mockRepository
            .Setup(repo => repo.ExistsByIdAsync(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(repo => repo.ExistsByNameAsync(request.Name, 1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<CarBrand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Id.Should().Be(1);
        response.Data.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task Update_WithInvalidModel_ShouldReturnBadRequest()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.Update(1, request, CancellationToken.None);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        _mockRepository
            .Setup(repo => repo.ExistsByIdAsync(999, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(999, request, CancellationToken.None);

        // Assert
        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = notFound.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Update_WithDuplicateName_ShouldReturnConflict()
    {
        // Arrange
        var request = CreateValidUpdateRequest();
        _mockRepository
            .Setup(repo => repo.ExistsByIdAsync(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(repo => repo.ExistsByNameAsync(request.Name, 1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(1, request, CancellationToken.None);

        // Assert
        var conflict = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var response = conflict.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("already exists");
    }

    // Delete tests

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnOk()
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().Be("Deleted");
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999, CancellationToken.None);

        // Assert
        var notFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = notFound.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeFalse();
    }

    // Constructor validation tests

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new MarcasAutosController(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("repository");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new MarcasAutosController(_mockRepository.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // Sorting validation tests

    [Fact]
    public async Task GetAll_WithInvalidSortBy_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetAll(
            sortBy: "invalidColumn",
            cancellationToken: CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid sorting parameter.");
        response.Errors.Should().ContainKey("sortBy");
    }

    [Fact]
    public async Task GetAll_WithInvalidSortDirection_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetAll(
            sortDirection: "sideways",
            cancellationToken: CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<ApiResponse<CarBrandDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid sorting parameter.");
        response.Errors.Should().ContainKey("sortDirection");
    }

    [Theory]
    [InlineData("name", "asc")]
    [InlineData("country", "desc")]
    [InlineData("foundedYear", "asc")]
    [InlineData("isLuxury", "desc")]
    [InlineData("headquarters", "asc")]
    public async Task GetAll_WithValidSortParameters_ShouldReturnOk(string sortBy, string sortDirection)
    {
        // Arrange
        var brands = GetTestBrands();
        _mockRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CarBrandQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((brands, brands.Count));

        // Act
        var result = await _controller.GetAll(
            sortBy: sortBy,
            sortDirection: sortDirection,
            cancellationToken: CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockRepository.Verify(repo => repo.GetAllAsync(
            It.Is<CarBrandQuery>(q =>
                q.SortBy == sortBy &&
                q.SortDirection == sortDirection),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Test data

    /// <summary>
    /// Generates consistent test data for the tests.
    /// </summary>
    private static List<CarBrand> GetTestBrands()
    {
        return new List<CarBrand>
        {
            new() { Id = 1, Name = "Toyota", CountryOfOrigin = "Japan", FoundedYear = 1937, Website = "https://www.toyota.com", IsLuxury = false, Headquarters = "Toyota City, Aichi", IsActive = true },
            new() { Id = 2, Name = "Ford", CountryOfOrigin = "United States", FoundedYear = 1903, Website = "https://www.ford.com", IsLuxury = false, Headquarters = "Dearborn, Michigan", IsActive = true },
            new() { Id = 3, Name = "BMW", CountryOfOrigin = "Germany", FoundedYear = 1916, Website = "https://www.bmw.com", IsLuxury = true, Headquarters = "Munich, Bavaria", IsActive = true }
        };
    }

    private static CarBrandCreateRequest CreateValidCreateRequest()
    {
        return new CarBrandCreateRequest
        {
            Name = "Mazda",
            CountryOfOrigin = "Japan",
            FoundedYear = 1920,
            Website = "https://www.mazda.com",
            IsLuxury = false,
            Headquarters = "Hiroshima"
        };
    }

    private static CarBrandUpdateRequest CreateValidUpdateRequest()
    {
        return new CarBrandUpdateRequest
        {
            Name = "Mazda",
            CountryOfOrigin = "Japan",
            FoundedYear = 1920,
            Website = "https://www.mazda.com",
            IsLuxury = false,
            Headquarters = "Hiroshima"
        };
    }
}
