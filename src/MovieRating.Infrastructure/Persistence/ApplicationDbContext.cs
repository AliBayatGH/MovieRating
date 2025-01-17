using Microsoft.EntityFrameworkCore;
using MovieRating.Domain.Entities;
using MovieRating.Infrastructure.Interceptors;
using System.Reflection;

namespace MovieRating.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly QueryLoggingInterceptor _queryLoggingInterceptor;

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Rating> Ratings => Set<Rating>();

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        QueryLoggingInterceptor queryLoggingInterceptor)
        : base(options)
    {
        _queryLoggingInterceptor = queryLoggingInterceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_queryLoggingInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}