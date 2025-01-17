using FluentValidation;
using Microsoft.Extensions.Logging;
using MovieRating.Application.DTOs;
using MovieRating.Domain.Entities;
using MovieRating.Domain.Exceptions;
using MovieRating.Domain.Models;
using MovieRating.Domain.Repositories;

namespace MovieRating.Application.Services;

public interface IMovieService
{
    Task<MovieDto> CreateMovieAsync(CreateMovieDto dto, CancellationToken cancellationToken = default);
    Task<MovieDto?> GetMovieAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MovieDto>> GetMoviesAsync(MovieFilter filter, CancellationToken cancellationToken = default);
    Task<MovieDto> RateMovieAsync(Guid id, RateMovieDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken = default);
}

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateMovieDto> _createMovieValidator;
    private readonly ILogger<MovieService> _logger;

    public MovieService(
        IMovieRepository movieRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateMovieDto> createMovieValidator,
        ILogger<MovieService> logger)
    {
        _movieRepository = movieRepository;
        _unitOfWork = unitOfWork;
        _createMovieValidator = createMovieValidator;
        _logger = logger;
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createMovieValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var movie = new Movie(dto.Title, dto.ReleaseYear, dto.Genre, dto.Director);

        await _movieRepository.AddAsync(movie, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(movie);
    }

    public async Task<MovieDto?> GetMovieAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken);
        return movie != null ? MapToDto(movie) : null;
    }

    public async Task<IEnumerable<MovieDto>> GetMoviesAsync(MovieFilter filter, CancellationToken cancellationToken = default)
    {
        var movies = await _movieRepository.GetAllAsync(filter, cancellationToken);
        return movies.Select(MapToDto);
    }

    public async Task<MovieDto> RateMovieAsync(Guid id, RateMovieDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {id} not found");

        var rating = new Rating(dto.Rating, id, userId);
        movie.AddRating(rating);

        //await _movieRepository.UpdateAsync(movie, cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {

            throw;
        }
        return MapToDto(movie);
    }

    public async Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken);
        if (movie == null)
            throw new NotFoundException($"Movie with ID {id} not found");

        movie.SoftDelete();
        await _movieRepository.UpdateAsync(movie, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static MovieDto MapToDto(Movie movie)
    {
        return new MovieDto(
            movie.Id,
            movie.Title,
            movie.ReleaseYear,
            movie.Genre,
            movie.Director,
            movie.GetAverageRating(),
            movie.GetTotalRatingsCount(),
            movie.CreatedAt,
            movie.UpdatedAt
        );
    }
}