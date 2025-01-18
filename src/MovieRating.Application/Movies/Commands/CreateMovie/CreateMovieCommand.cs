using MediatR;
using MovieRating.Application.DTOs;

namespace MovieRating.Application.Movies.Commands.CreateMovie;

public record CreateMovieCommand(string Title, int ReleaseYear, string Genre, string Director) : IRequest<MovieDto>;