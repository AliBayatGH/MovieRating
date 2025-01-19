using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRating.Application.Queries.Movies.GetMovies;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Repositories;
using MovieRating.Domain.ValueObjects;
using Xunit;

namespace MovieRating.UnitTests.Application.Movies.Queries;

public class GetMoviesQueryHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly Mock<ILogger<GetMoviesQueryHandler>> _loggerMock;
    private readonly GetMoviesQueryHandler _handler;

    public GetMoviesQueryHandlerTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _loggerMock = new Mock<ILogger<GetMoviesQueryHandler>>();
        _handler = new GetMoviesQueryHandler(
            _movieRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithFilter_ShouldReturnFilteredMovies()
    {
        // Arrange
        var filter = new MovieFilter(titleSearch: "Test");
        var movies = new List<Movie>
        {
            new Movie("Test Movie", 2024, "Action", "John Doe")
        };
        _movieRepositoryMock.Setup(x => x.GetAllAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movies);

        var query = new GetMoviesQuery(filter);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Test Movie");
    }
}