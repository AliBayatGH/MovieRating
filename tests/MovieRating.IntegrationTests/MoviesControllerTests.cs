using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MovieRating.Application.DTOs;
using MovieRating.Infrastructure.Persistence;
using MovieRating.IntegrationTests.Fixtures;
using Xunit;

namespace MovieRating.IntegrationTests;

public class MoviesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MoviesControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
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
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(movie.Title, result.Title);
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(movie.Title, result.Title);
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(5, result.AverageRating);
        Assert.Equal(1, result.TotalRatings);
    }
}