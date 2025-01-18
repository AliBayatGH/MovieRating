using MediatR;
using Microsoft.Extensions.Logging;
using MovieRating.Application.DTOs;
using MovieRating.Domain.Exceptions;
using MovieRating.Domain.Repositories;

namespace MovieRating.Application.Movies.Queries.GetMovie;

public class GetMovieQueryHandler : IRequestHandler<GetMovieQuery, MovieDto>
{
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<GetMovieQueryHandler> _logger;

    public GetMovieQueryHandler(IMovieRepository movieRepository, ILogger<GetMovieQueryHandler> logger)
    {
        _movieRepository = movieRepository;
        _logger = logger;
    }

    public async Task<MovieDto> Handle(GetMovieQuery query, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(query.Id, cancellationToken);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {query.Id} not found");

        return new MovieDto(
            movie.Id,
            movie.Title,
            movie.ReleaseYear,
            movie.Genre,
            movie.Director,
            movie.GetAverageRating(),
            movie.GetTotalRatingsCount(),
            movie.CreatedAt,
            movie.UpdatedAt);
    }
}