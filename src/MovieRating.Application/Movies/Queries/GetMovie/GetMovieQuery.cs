using MediatR;
using MovieRating.Application.DTOs;

namespace MovieRating.Application.Movies.Queries.GetMovie;

public record GetMovieQuery(Guid Id) : IRequest<MovieDto>;