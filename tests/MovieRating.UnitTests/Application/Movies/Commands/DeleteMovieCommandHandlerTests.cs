using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRating.Application.Commands.Movies.DeleteMovie;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Exceptions;
using MovieRating.Domain.Repositories;
using Xunit;

namespace MovieRating.UnitTests.Application.Movies.Commands;

public class DeleteMovieCommandHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<DeleteMovieCommandHandler>> _loggerMock;
    private readonly DeleteMovieCommandValidator _validator;
    private readonly DeleteMovieCommandHandler _handler;

    public DeleteMovieCommandHandlerTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<DeleteMovieCommandHandler>>();
        _validator = new DeleteMovieCommandValidator();
        _handler = new DeleteMovieCommandHandler(
            _movieRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingMovie_ShouldSoftDelete()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var movie = new Movie("Test Movie", 2024, "Action", "John Doe");
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        var command = new DeleteMovieCommand(movieId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        movie.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingMovie_ShouldThrowNotFoundException()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        _movieRepositoryMock.Setup(x => x.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var command = new DeleteMovieCommand(movieId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Movie with ID {movieId} not found");
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new DeleteMovieCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}