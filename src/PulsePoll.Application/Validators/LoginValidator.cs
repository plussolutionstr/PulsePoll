using FluentValidation;
using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MaximumLength(128);
    }
}
