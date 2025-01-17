using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRating.Application.DTOs;
using MovieRating.Application.Services;
using MovieRating.Domain.Models;
using System.Security.Claims;

namespace MovieRating.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMovie(
        CreateMovieDto dto,
        CancellationToken cancellationToken)
    {
        var movie = await _movieService.CreateMovieAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = 60)] // Cache for 1 minute
    public async Task<IActionResult> GetMovie(
        Guid id,
        CancellationToken cancellationToken)
    {
        var movie = await _movieService.GetMovieAsync(id, cancellationToken);
        if (movie == null)
            return NotFound();

        return Ok(movie);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MovieDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovies(
        [FromQuery] string? titleSearch,
        [FromQuery] string? genre,
        [FromQuery] int? year,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var filter = new MovieFilter(titleSearch, genre, year, sortBy, sortDescending, page, pageSize);
        var movies = await _movieService.GetMoviesAsync(filter, cancellationToken);

        return Ok(movies);
    }

    [HttpPost("{id:guid}/ratings")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RateMovie(
        Guid id,
        RateMovieDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromToken(); 
        var movie = await _movieService.RateMovieAsync(id, dto, userId, cancellationToken);

        return Ok(movie);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMovie(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _movieService.DeleteMovieAsync(id, cancellationToken);

        return NoContent();
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}