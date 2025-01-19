using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRating.Application.Commands.Movies.CreateMovie;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Repositories;
using Xunit;

namespace MovieRating.UnitTests.Application.Movies.Commands;

public class CreateMovieCommandHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateMovieCommandHandler>> _loggerMock;
    private readonly CreateMovieCommandValidator _validator;
    private readonly CreateMovieCommandHandler _handler;

    public CreateMovieCommandHandlerTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateMovieCommandHandler>>();
        _validator = new CreateMovieCommandValidator();
        _handler = new CreateMovieCommandHandler(
            _movieRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateMovie()
    {
        // Arrange
        var command = new CreateMovieCommand("Test Movie", 2024, "Action", "John Doe");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
    public void Validate_WithInvalidCommand_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new CreateMovieCommand(
            string.Empty,
            1800,
            string.Empty,
            string.Empty
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.ReleaseYear);
        result.ShouldHaveValidationErrorFor(x => x.Genre);
        result.ShouldHaveValidationErrorFor(x => x.Director);
    }
}