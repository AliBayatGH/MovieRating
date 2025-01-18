using FluentValidation;

namespace MovieRating.Application.Movies.Commands.RateMovie;

public class RateMovieCommandValidator : AbstractValidator<RateMovieCommand>
{
    public RateMovieCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");
    }
}