using MediatR;
using Microsoft.Extensions.Logging;
using MovieRating.Application.DTOs;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Exceptions;
using MovieRating.Domain.Repositories;

namespace MovieRating.Application.Commands.Movies.RateMovie;

public class RateMovieCommandHandler : IRequestHandler<RateMovieCommand, MovieDto>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RateMovieCommandHandler> _logger;

    public RateMovieCommandHandler(
        IMovieRepository movieRepository,
        IUnitOfWork unitOfWork,
        ILogger<RateMovieCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MovieDto> Handle(RateMovieCommand command, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(command.MovieId, cancellationToken);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {command.MovieId} not found");

        var rating = new Rating(command.Rating, command.MovieId, command.UserId);
        movie.AddRating(rating);
        await _movieRepository.UpdateAsync(movie);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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