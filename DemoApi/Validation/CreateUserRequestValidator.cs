using DemoApi.Controllers;
using DemoApi.Domain;
using JV.ResultUtilities.FluentValidation;

namespace DemoApi.Validation;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage(ValidationKeys.User.NameCannotBeEmmpty)
            .MaxLength(100)
                .WithMessage(ValidationKeys.User.NameTooLong, u => new { Name = u.Name, MaxLength = 100 });

        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(email => email.Contains("@"))
                .WithMessage(ValidationKeys.User.EmailInvalid, u => u.Email);
    }
}
