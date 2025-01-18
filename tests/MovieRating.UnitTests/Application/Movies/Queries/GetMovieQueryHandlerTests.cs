using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRating.Application.Movies.Queries.GetMovie;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Exceptions;
using MovieRating.Domain.Repositories;
using Xunit;

namespace MovieRating.UnitTests.Application.Movies.Queries;

public class GetMovieQueryHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly Mock<ILogger<GetMovieQueryHandler>> _loggerMock;
    private readonly GetMovieQueryHandler _handler;

    public GetMovieQueryHandlerTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _loggerMock = new Mock<ILogger<GetMovieQueryHandler>>();
        _handler = new GetMovieQueryHandler(
            _movieRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingId_ShouldReturnMovie()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movie = new Movie("Test Movie", 2024, "Action", "John Doe");
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        var query = new GetMovieQuery(movieId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(movie.Title);
    }

    [Fact]
    public async Task Handle_WithNonExistingId_ShouldThrowNotFoundException()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var query = new GetMovieQuery(movieId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Movie with ID {movieId} not found");
    }
}