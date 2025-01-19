using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRating.Application.Commands.Movies.RateMovie;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Repositories;
using Xunit;

namespace MovieRating.UnitTests.Application.Movies.Commands;

public class RateMovieCommandHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RateMovieCommandHandler>> _loggerMock;
    private readonly RateMovieCommandValidator _validator;
    private readonly RateMovieCommandHandler _handler;

    public RateMovieCommandHandlerTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RateMovieCommandHandler>>();
        _validator = new RateMovieCommandValidator();
        _handler = new RateMovieCommandHandler(
            _movieRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRating_ShouldUpdateMovie()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var movie = new Movie("Test Movie", 2024, "Action", "John Doe");

        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        var command = new RateMovieCommand(movieId, 5, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
    public void Validate_WithInvalidCommand_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new RateMovieCommand(
            Guid.Empty,
            0,
            Guid.Empty
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MovieId);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}