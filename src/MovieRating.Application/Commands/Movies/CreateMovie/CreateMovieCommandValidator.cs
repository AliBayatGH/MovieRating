using FluentValidation;

namespace MovieRating.Application.Commands.Movies.CreateMovie;

public class CreateMovieCommandValidator : AbstractValidator<CreateMovieCommand>
{
    public CreateMovieCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ReleaseYear)
            .GreaterThan(1888) // First movie ever made
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 5); // Allow for upcoming movies

        RuleFor(x => x.Genre)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Director)
            .NotEmpty()
            .MaximumLength(100);
    }
}