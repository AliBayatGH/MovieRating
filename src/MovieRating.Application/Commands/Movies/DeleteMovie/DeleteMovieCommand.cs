using MediatR;

namespace MovieRating.Application.Commands.Movies.DeleteMovie;

public record DeleteMovieCommand(Guid Id) : IRequest;