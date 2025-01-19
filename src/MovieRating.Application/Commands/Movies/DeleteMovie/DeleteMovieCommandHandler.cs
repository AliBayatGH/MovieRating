using MediatR;
using Microsoft.Extensions.Logging;
using MovieRating.Domain.Exceptions;
using MovieRating.Domain.Repositories;

namespace MovieRating.Application.Commands.Movies.DeleteMovie;

public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteMovieCommandHandler> _logger;

    public DeleteMovieCommandHandler(
        IMovieRepository movieRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteMovieCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteMovieCommand command, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(command.Id, cancellationToken);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {command.Id} not found");

        movie.SoftDelete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}