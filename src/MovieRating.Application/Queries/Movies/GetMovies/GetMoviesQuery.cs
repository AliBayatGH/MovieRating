using MediatR;
using MovieRating.Application.DTOs;
using MovieRating.Domain.ValueObjects;

namespace MovieRating.Application.Queries.Movies.GetMovies;

public record GetMoviesQuery(MovieFilter Filter) : IRequest<IEnumerable<MovieDto>>;