using MediatR;
using Microsoft.Extensions.Logging;
using MovieRating.Application.DTOs;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Repositories;

namespace MovieRating.Application.Commands.Movies.CreateMovie;

public class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, MovieDto>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateMovieCommandHandler> _logger;

    public CreateMovieCommandHandler(
        IMovieRepository movieRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateMovieCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MovieDto> Handle(CreateMovieCommand command, CancellationToken cancellationToken)
    {
        var movie = new Movie(command.Title, command.ReleaseYear, command.Genre, command.Director);

        await _movieRepository.AddAsync(movie, cancellationToken);
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