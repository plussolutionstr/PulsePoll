using FluentValidation;
using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Validators;

public class AddBankAccountValidator : AbstractValidator<AddBankAccountDto>
{
    public AddBankAccountValidator()
    {
        RuleFor(x => x.BankId)
            .GreaterThan(0).WithMessage("Banka seçimi zorunludur.");

        RuleFor(x => x.Iban)
            .NotEmpty().WithMessage("IBAN zorunludur.")
            .Length(26).WithMessage("Türk IBAN numarası 26 karakter olmalıdır.")
            .Matches(@"^TR\d{24}$").WithMessage("Geçerli bir Türk IBAN numarası giriniz (TR + 24 rakam).");
    }
}
