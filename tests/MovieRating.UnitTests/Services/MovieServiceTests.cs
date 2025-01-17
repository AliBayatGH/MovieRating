using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRating.Application.DTOs;
using MovieRating.Application.Services;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Exceptions;
using MovieRating.Domain.Models;
using MovieRating.Domain.Repositories;
using Xunit;

namespace MovieRating.UnitTests.Services;

public class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<CreateMovieDto>> _validatorMock;
    private readonly Mock<ILogger<MovieService>> _loggerMock;
    private readonly MovieService _sut;

    public MovieServiceTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validatorMock = new Mock<IValidator<CreateMovieDto>>();
        _loggerMock = new Mock<ILogger<MovieService>>();

        _sut = new MovieService(
            _movieRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateMovie_WithValidData_ShouldCreateMovie()
    {
        // Arrange
        var dto = new CreateMovieDto("Test Movie", 2024, "Action", "John Doe");
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreateMovieDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _sut.CreateMovieAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.ReleaseYear.Should().Be(dto.ReleaseYear);
        result.Genre.Should().Be(dto.Genre);
        result.Director.Should().Be(dto.Director);

        _movieRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMovie_WithExistingId_ShouldReturnMovie()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movie = new Movie("Test Movie", 2024, "Action", "John Doe");
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        // Act
        var result = await _sut.GetMovieAsync(movieId);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(movie.Title);
    }

    [Fact]
    public async Task GetMovie_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        // Act
        var result = await _sut.GetMovieAsync(movieId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMovies_WithFilter_ShouldReturnFilteredMovies()
    {
        // Arrange
        var filter = new MovieFilter(titleSearch: "Test");
        var movies = new List<Movie>
        {
            new Movie("Test Movie", 2024, "Action", "John Doe")
        };
        _movieRepositoryMock.Setup(x => x.GetAllAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movies);

        // Act
        var result = await _sut.GetMoviesAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Test Movie");
    }

    [Fact]
    public async Task RateMovie_WithValidRating_ShouldUpdateMovie()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var movie = new Movie("Test Movie", 2024, "Action", "John Doe");
        var dto = new RateMovieDto(5);

        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        // Act
        var result = await _sut.RateMovieAsync(movieId, dto, userId);

        // Assert
        result.Should().NotBeNull();
        result.AverageRating.Should().Be(5);
        result.TotalRatings.Should().Be(1);

        _movieRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteMovie_WithExistingMovie_ShouldSoftDelete()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movie = new Movie("Test Movie", 2024, "Action", "John Doe");
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        // Act
        await _sut.DeleteMovieAsync(movieId);

        // Assert
        _movieRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Movie>(m => m.IsDeleted), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteMovie_WithNonExistingMovie_ShouldThrowNotFoundException()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        // Act
        var act = () => _sut.DeleteMovieAsync(movieId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Movie with ID {movieId} not found");
    }
}