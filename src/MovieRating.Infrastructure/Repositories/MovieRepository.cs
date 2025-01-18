using Microsoft.EntityFrameworkCore;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Models;
using MovieRating.Domain.Repositories;
using MovieRating.Infrastructure.Persistence;

namespace MovieRating.Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly ApplicationDbContext _context;

    public MovieRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .Include(m => m.Ratings)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(MovieFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Movies
            .Include(m => m.Ratings)
            .Where(m => !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.TitleSearch))
            query = query.Where(m => m.Title.Contains(filter.TitleSearch));

        if (!string.IsNullOrWhiteSpace(filter.Genre))
            query = query.Where(m => m.Genre == filter.Genre);

        if (filter.Year.HasValue)
            query = query.Where(m => m.ReleaseYear == filter.Year);

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "title" => filter.SortDescending ?
                query.OrderByDescending(m => m.Title) :
                query.OrderBy(m => m.Title),
            "year" => filter.SortDescending ?
                query.OrderByDescending(m => m.ReleaseYear) :
                query.OrderBy(m => m.ReleaseYear),
            "rating" => filter.SortDescending ?
                query.OrderByDescending(m => m.Ratings.Average(r => r.Value)) :
                query.OrderBy(m => m.Ratings.Average(r => r.Value)),
            _ => query.OrderByDescending(m => m.CreatedAt)
        };

        return await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await _context.Movies.AddAsync(movie, cancellationToken);
    }

    public async Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        // Attach the movie if not already tracked
        _context.Movies.Attach(movie);

        // Ensure all ratings are tracked and marked appropriately
        foreach (var rating in movie.Ratings.Where(r => _context.Entry(r).State == EntityState.Detached))
        {
            _context.Entry(rating).State = EntityState.Added;
        }

        await Task.CompletedTask; // Maintain async signature
    }


    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies.AnyAsync(m => m.Id == id && !m.IsDeleted, cancellationToken);
    }
}