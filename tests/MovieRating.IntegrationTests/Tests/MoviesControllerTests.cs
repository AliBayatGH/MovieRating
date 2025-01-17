using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MovieRating.Application.DTOs;
using MovieRating.IntegrationTests.Fixtures;
using Xunit;

namespace MovieRating.IntegrationTests.Tests;

public class MoviesControllerTests : IClassFixture<SharedTestContext>
{
    private readonly HttpClient _client;
    private readonly SharedTestContext _factory;

    public MoviesControllerTests(SharedTestContext factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task CreateMovie_WithValidData_ReturnsCreated()
    {
        // Arrange
        var movie = new CreateMovieDto(
            "Test Movie",
            2024,
            "Action",
            "John Doe"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/movies", movie);
        var result = await response.Content.ReadFromJsonAsync<MovieDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result!.Title.Should().Be(movie.Title);
    }

    [Fact]
    public async Task GetMovie_WithExistingMovie_ReturnsMovie()
    {
        // Arrange
        var movie = new CreateMovieDto(
            "Test Movie",
            2024,
            "Action",
            "John Doe"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/movies", movie);
        var createdMovie = await createResponse.Content.ReadFromJsonAsync<MovieDto>();

        // Act
        var response = await _client.GetAsync($"/api/movies/{createdMovie!.Id}");
        var result = await response.Content.ReadFromJsonAsync<MovieDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Title.Should().Be(movie.Title);
    }

    [Fact]
    public async Task GetMovies_WithFilters_ReturnsFilteredMovies()
    {
        // Arrange
        var movies = new[]
        {
            new CreateMovieDto("Action Movie", 2024, "Action", "John Doe"),
            new CreateMovieDto("Comedy Movie", 2024, "Comedy", "Jane Doe"),
            new CreateMovieDto("Old Action", 2020, "Action", "John Doe")
        };

        foreach (var movie in movies)
        {
            await _client.PostAsJsonAsync("/api/movies", movie);
        }

        // Act
        var response = await _client.GetAsync("/api/movies?genre=Action&year=2024");
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<MovieDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Should().HaveCount(1);
        result.First().Title.Should().Be("Action Movie");
    }

    [Fact]
    public async Task RateMovie_WithValidRating_ReturnsUpdatedMovie()
    {
        // Arrange
        var movie = new CreateMovieDto(
            "Test Movie",
            2024,
            "Action",
            "John Doe"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/movies", movie);
        var createdMovie = await createResponse.Content.ReadFromJsonAsync<MovieDto>();
        var rating = new RateMovieDto(5);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/movies/{createdMovie!.Id}/ratings", rating);
        var result = await response.Content.ReadFromJsonAsync<MovieDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.AverageRating.Should().Be(5);
        result.TotalRatings.Should().Be(1);
    }

    [Fact]
    public async Task DeleteMovie_WithExistingMovie_ReturnsNoContent()
    {
        // Arrange
        var movie = new CreateMovieDto(
            "Test Movie",
            2024,
            "Action",
            "John Doe"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/movies", movie);
        var createdMovie = await createResponse.Content.ReadFromJsonAsync<MovieDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/movies/{createdMovie!.Id}");
        var getResponse = await _client.GetAsync($"/api/movies/{createdMovie.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}