namespace MovieRating.Domain.Entities;

public class Rating
{
    public Guid Id { get; private set; }
    public int Value { get; private set; }
    public Guid MovieId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Rating() { } // For EF Core

    public Rating(int value, Guid movieId, Guid userId)
    {
        if (value < 1 || value > 5)
            throw new ArgumentException("Rating must be between 1 and 5");

        Id = Guid.NewGuid();
        Value = value;
        MovieId = movieId;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }
}