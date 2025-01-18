using MediatR;
using MovieRating.Application.DTOs;

namespace MovieRating.Application.Movies.Commands.RateMovie;

public record RateMovieCommand(Guid MovieId, int Rating, Guid UserId) : IRequest<MovieDto>;