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
            .Must(BeValidTurkishIban).WithMessage("Geçerli bir Türk IBAN numarası giriniz.");
    }

    private static bool BeValidTurkishIban(string? rawIban)
    {
        var compact = new string((rawIban ?? string.Empty)
            .Where(char.IsLetterOrDigit)
            .ToArray())
            .ToUpperInvariant();

        if (!compact.StartsWith("TR", StringComparison.Ordinal))
            compact = $"TR{compact}";

        return compact.Length == 26 &&
               compact.StartsWith("TR", StringComparison.Ordinal) &&
               compact.Skip(2).All(char.IsDigit);
    }
}
