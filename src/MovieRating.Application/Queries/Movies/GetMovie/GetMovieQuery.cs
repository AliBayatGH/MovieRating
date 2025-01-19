using MediatR;
using MovieRating.Application.DTOs;

namespace MovieRating.Application.Queries.Movies.GetMovie;

public record GetMovieQuery(Guid Id) : IRequest<MovieDto>;