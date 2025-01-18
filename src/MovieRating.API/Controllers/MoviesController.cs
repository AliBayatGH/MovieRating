using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRating.Application.DTOs;
using MovieRating.Application.Movies.Commands.CreateMovie;
using MovieRating.Application.Movies.Commands.DeleteMovie;
using MovieRating.Application.Movies.Commands.RateMovie;
using MovieRating.Application.Movies.Queries.GetMovie;
using MovieRating.Application.Movies.Queries.GetMovies;
using MovieRating.Domain.Models;
using System.Security.Claims;

namespace MovieRating.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")] // for backward compatibility
[Authorize]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
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
        var result = await _mediator.Send(new GetMoviesQuery(filter), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MovieDto>> GetMovie(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMovieQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MovieDto>> CreateMovie(CreateMovieDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateMovieCommand(dto.Title, dto.ReleaseYear, dto.Genre, dto.Director);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMovie), new { id = result.Id }, result);
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
        var command = new RateMovieCommand(id, dto.Rating, userId); 
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMovie(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMovieCommand(id), cancellationToken);
        return NoContent();
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}