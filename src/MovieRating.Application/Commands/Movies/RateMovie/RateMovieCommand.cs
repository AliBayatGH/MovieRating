using MediatR;
using MovieRating.Application.DTOs;

namespace MovieRating.Application.Commands.Movies.RateMovie;

public record RateMovieCommand(Guid MovieId, int Rating, Guid UserId) : IRequest<MovieDto>;