using Microsoft.EntityFrameworkCore;
using MovieRating.Domain.Entities;
using MovieRating.Infrastructure.Persistence;
using MovieRating.IntegrationTests.Fixtures;

namespace MovieRating.IntegrationTests.Helpers;

public static class DatabaseHelper
{
    public static async Task<Movie> CreateMovieInDatabase(SharedTestContext fixture, Movie movie)
    {
        var context = fixture.GetDbContext();
        context.Movies.Add(movie);
        await context.SaveChangesAsync();
        return movie;
    }

    public static async Task<Movie?> GetMovieFromDatabase(SharedTestContext fixture, Guid movieId, bool ignoreQueryFilters = true)
    {
        var context = fixture.GetDbContext();
        var query = context.Movies.AsQueryable();

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(x => x.Id == movieId);
    }

    public static async Task ClearDatabase(SharedTestContext fixture)
    {
        var context = fixture.GetDbContext();
        var movies = await context.Movies.IgnoreQueryFilters().ToListAsync();
        var ratings = await context.Ratings.IgnoreQueryFilters().ToListAsync();

        context.Movies.RemoveRange(movies);
        context.Ratings.RemoveRange(ratings);
        await context.SaveChangesAsync();
    }
}