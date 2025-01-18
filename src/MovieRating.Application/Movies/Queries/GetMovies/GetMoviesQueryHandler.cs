using MediatR;
using Microsoft.Extensions.Logging;
using MovieRating.Application.DTOs;
using MovieRating.Domain.Repositories;

namespace MovieRating.Application.Movies.Queries.GetMovies;

public class GetMoviesQueryHandler : IRequestHandler<GetMoviesQuery, IEnumerable<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<GetMoviesQueryHandler> _logger;

    public GetMoviesQueryHandler(IMovieRepository movieRepository, ILogger<GetMoviesQueryHandler> logger)
    {
        _movieRepository = movieRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<MovieDto>> Handle(GetMoviesQuery query, CancellationToken cancellationToken)
    {
        var movies = await _movieRepository.GetAllAsync(query.Filter, cancellationToken);

        return movies.Select(movie => new MovieDto(
            movie.Id,
            movie.Title,
            movie.ReleaseYear,
            movie.Genre,
            movie.Director,
            movie.GetAverageRating(),
            movie.GetTotalRatingsCount(),
            movie.CreatedAt,
            movie.UpdatedAt));
    }
}