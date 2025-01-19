using MovieRating.Domain.Entities;
using MovieRating.Domain.ValueObjects;

namespace MovieRating.Domain.Repositories;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetAllAsync(MovieFilter filter, CancellationToken cancellationToken = default);
    Task AddAsync(Movie movie, CancellationToken cancellationToken = default);
    Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}