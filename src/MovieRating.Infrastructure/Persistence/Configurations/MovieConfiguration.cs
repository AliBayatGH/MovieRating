using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRating.Domain.Entities;

namespace MovieRating.Infrastructure.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Genre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Director)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(e => e.Ratings)
            .WithOne()
            .HasForeignKey(e => e.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter for soft delete
        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}