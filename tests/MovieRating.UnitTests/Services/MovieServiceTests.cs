using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRating.Application.DTOs;
using MovieRating.Application.Movies.Commands.CreateMovie;
using MovieRating.Application.Movies.Commands.DeleteMovie;
using MovieRating.Application.Movies.Commands.RateMovie;
using MovieRating.Application.Movies.Queries.GetMovie;
using MovieRating.Application.Movies.Queries.GetMovies;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Exceptions;
using MovieRating.Domain.Models;
using MovieRating.Domain.Repositories;
using Xunit;

namespace MovieRating.UnitTests.Handlers;

public class MovieHandlersTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateMovieCommandHandler>> _createLoggerMock;
    private readonly Mock<ILogger<GetMovieQueryHandler>> _getLoggerMock;
    private readonly Mock<ILogger<GetMoviesQueryHandler>> _getAllLoggerMock;
    private readonly Mock<ILogger<DeleteMovieCommandHandler>> _deleteLoggerMock;
    private readonly Mock<ILogger<RateMovieCommandHandler>> _rateLoggerMock;

    public MovieHandlersTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _createLoggerMock = new Mock<ILogger<CreateMovieCommandHandler>>();
        _getLoggerMock = new Mock<ILogger<GetMovieQueryHandler>>();
        _getAllLoggerMock = new Mock<ILogger<GetMoviesQueryHandler>>();
        _deleteLoggerMock = new Mock<ILogger<DeleteMovieCommandHandler>>();
        _rateLoggerMock = new Mock<ILogger<RateMovieCommandHandler>>();
    }

    [Fact]
    public async Task CreateMovie_WithValidData_ShouldCreateMovie()
    {
        // Arrange
        var handler = new CreateMovieCommandHandler(
            _movieRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _createLoggerMock.Object);
            
        var command = new CreateMovieCommand("Test Movie", 2024, "Action", "John Doe");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
        result.ReleaseYear.Should().Be(command.ReleaseYear);
        result.Genre.Should().Be(command.Genre);
        result.Director.Should().Be(command.Director);

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
        var handler = new GetMovieQueryHandler(
            _movieRepositoryMock.Object,
            _getLoggerMock.Object);
            
        var movieId = Guid.NewGuid();
        var movie = new Movie("Test Movie", 2024, "Action", "John Doe");
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        var query = new GetMovieQuery(movieId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(movie.Title);
    }

    [Fact]
    public async Task GetMovie_WithNonExistingId_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new GetMovieQueryHandler(
            _movieRepositoryMock.Object,
            _getLoggerMock.Object);
            
        var movieId = Guid.NewGuid();
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var query = new GetMovieQuery(movieId);

        // Act
        var act = () => handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Movie with ID {movieId} not found");
    }

    [Fact]
    public async Task GetMovies_WithFilter_ShouldReturnFilteredMovies()
    {
        // Arrange
        var handler = new GetMoviesQueryHandler(
            _movieRepositoryMock.Object,
            _getAllLoggerMock.Object);
            
        var filter = new MovieFilter(titleSearch: "Test");
        var movies = new List<Movie>
        {
            new Movie("Test Movie", 2024, "Action", "John Doe")
        };
        _movieRepositoryMock.Setup(x => x.GetAllAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movies);

        var query = new GetMoviesQuery(filter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Test Movie");
    }

    [Fact]
    public async Task RateMovie_WithValidRating_ShouldUpdateMovie()
    {
        // Arrange
        var handler = new RateMovieCommandHandler(
            _movieRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _rateLoggerMock.Object);
            
        var movieId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var movie = new Movie("Test Movie", 2024, "Action", "John Doe");

        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        var command = new RateMovieCommand(movieId, 5, userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

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
        var handler = new DeleteMovieCommandHandler(
            _movieRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _deleteLoggerMock.Object);
            
        var movieId = Guid.NewGuid();
        var movie = new Movie("Test Movie", 2024, "Action", "John Doe");
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        var command = new DeleteMovieCommand(movieId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        movie.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteMovie_WithNonExistingMovie_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new DeleteMovieCommandHandler(
            _movieRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _deleteLoggerMock.Object);
            
        var movieId = Guid.NewGuid();
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var command = new DeleteMovieCommand(movieId);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Movie with ID {movieId} not found");
    }
}