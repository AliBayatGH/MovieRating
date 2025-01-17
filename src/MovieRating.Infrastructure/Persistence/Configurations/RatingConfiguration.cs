using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MovieRating.Domain.Entities;

namespace MovieRating.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Value)
            .IsRequired();

        builder.Property(e => e.MovieId)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();
    }
} 