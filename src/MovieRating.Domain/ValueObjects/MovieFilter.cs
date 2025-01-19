namespace MovieRating.Domain.Models;

public class MovieFilter
{
    public string? TitleSearch { get; init; }
    public string? Genre { get; init; }
    public int? Year { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public MovieFilter(
        string? titleSearch = null,
        string? genre = null,
        int? year = null,
        string? sortBy = null,
        bool sortDescending = false,
        int page = 1,
        int pageSize = 10)
    {
        TitleSearch = titleSearch;
        Genre = genre;
        Year = year;
        SortBy = sortBy;
        SortDescending = sortDescending;
        Page = page < 1 ? 1 : page;
        PageSize = pageSize < 1 ? 10 : pageSize;
    }
}