using FluentValidation;
using MovieRating.Application.DTOs;

namespace MovieRating.Application.Validators;

public class CreateMovieDtoValidator : AbstractValidator<CreateMovieDto>
{
    public CreateMovieDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ReleaseYear)
            .InclusiveBetween(1888, DateTime.UtcNow.Year + 1);

        RuleFor(x => x.Genre)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Director)
            .NotEmpty()
            .MaximumLength(100);
    }
}