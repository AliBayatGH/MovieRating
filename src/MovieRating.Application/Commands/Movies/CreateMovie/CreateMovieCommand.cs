using MediatR;
using MovieRating.Application.DTOs;

namespace MovieRating.Application.Commands.Movies.CreateMovie;

public record CreateMovieCommand(string Title, int ReleaseYear, string Genre, string Director) : IRequest<MovieDto>;