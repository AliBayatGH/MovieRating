using FluentValidation;

namespace MovieRating.Application.Movies.Commands.DeleteMovie;

public class DeleteMovieCommandValidator : AbstractValidator<DeleteMovieCommand>
{
    public DeleteMovieCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}