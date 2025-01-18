using MediatR;

namespace MovieRating.Application.Movies.Commands.DeleteMovie;

public record DeleteMovieCommand(Guid Id) : IRequest;