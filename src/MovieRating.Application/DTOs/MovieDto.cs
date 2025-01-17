namespace MovieRating.Application.DTOs;

public record MovieDto(
    Guid Id,
    string Title,
    int ReleaseYear,
    string Genre,
    string Director,
    double AverageRating,
    int TotalRatings,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateMovieDto(
    string Title,
    int ReleaseYear,
    string Genre,
    string Director
);

public record RateMovieDto(
    int Rating
);