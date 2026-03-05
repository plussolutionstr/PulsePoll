using FluentValidation;
using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterSubjectDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.RegistrationToken).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BirthDate).NotEmpty();
        RuleFor(x => x.CityId).GreaterThan(0);
        RuleFor(x => x.DistrictId).GreaterThan(0);
        RuleFor(x => x.ProfessionId).GreaterThan(0);
        RuleFor(x => x.EducationLevelId).GreaterThan(0);
        RuleFor(x => x.BankId).GreaterThan(0).When(x => x.BankId.HasValue);
        RuleFor(x => x.IBAN).MaximumLength(34).When(x => !string.IsNullOrEmpty(x.IBAN));
        RuleFor(x => x.IBANFullName).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.IBANFullName));
        RuleFor(x => x.KVKKApproval).Equal(true).WithMessage("KVKK onayı zorunludur.");
        RuleFor(x => x.HeadOfFamilyProfessionId).NotNull().When(x => !x.IsHeadOfFamily)
            .WithMessage("Hane reisi değilseniz hane reisinin mesleğini belirtmelisiniz.");
        RuleFor(x => x.HeadOfFamilyEducationLevelId).NotNull().When(x => !x.IsHeadOfFamily)
            .WithMessage("Hane reisi değilseniz hane reisinin eğitim düzeyini belirtmelisiniz.");
    }
}
