using MediatR;
using MovieRating.Application.DTOs;
using MovieRating.Domain.Models;

namespace MovieRating.Application.Movies.Queries.GetMovies;

public record GetMoviesQuery(MovieFilter Filter) : IRequest<IEnumerable<MovieDto>>;