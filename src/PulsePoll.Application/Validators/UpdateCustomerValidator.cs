using FluentValidation;
using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Validators;

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ünvan zorunludur.")
            .MaximumLength(200);

        RuleFor(x => x.ShortName)
            .NotEmpty().WithMessage("Kısa ad zorunludur.")
            .MaximumLength(100);

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithMessage("Vergi numarası zorunludur.")
            .Length(10, 11).WithMessage("Vergi numarası 10 veya 11 haneli olmalıdır.")
            .Matches(@"^\d+$").WithMessage("Vergi numarası sadece rakam içermelidir.");

        RuleFor(x => x.TaxOfficeId)
            .GreaterThan(0).WithMessage("Vergi dairesi seçilmelidir.");

        RuleFor(x => x.Phone1)
            .NotEmpty().WithMessage("Telefon zorunludur.")
            .MaximumLength(20);

        RuleFor(x => x.Phone2)
            .MaximumLength(20)
            .When(x => x.Phone2 != null);

        RuleFor(x => x.Mobile)
            .MaximumLength(20)
            .When(x => x.Mobile != null);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(100);

        RuleFor(x => x.CityId)
            .GreaterThan(0).WithMessage("Şehir seçilmelidir.");

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("İlçe seçilmelidir.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres zorunludur.")
            .MaximumLength(500);
    }
}
