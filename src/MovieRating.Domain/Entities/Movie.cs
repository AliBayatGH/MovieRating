namespace MovieRating.Domain.Entities;

public class Movie
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public int ReleaseYear { get; private set; }
    public string Genre { get; private set; }
    public string Director { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    private readonly List<Rating> _ratings = new();
    public IReadOnlyCollection<Rating> Ratings => _ratings.AsReadOnly();

    private Movie() { } // For EF Core

    public Movie(string title, int releaseYear, string genre, string director)
    {
        Id = Guid.NewGuid();
        Title = title;
        ReleaseYear = releaseYear;
        Genre = genre;
        Director = director;
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    public void AddRating(Rating rating)
    {
        if (Ratings.Any(r => r.UserId == rating.UserId))
            throw new InvalidOperationException("User has already rated this movie.");

        _ratings.Add(rating);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public double GetAverageRating()
    {
        return _ratings.Any() ? _ratings.Average(r => r.Value) : 0;
    }

    public int GetTotalRatingsCount()
    {
        return _ratings.Count;
    }
}