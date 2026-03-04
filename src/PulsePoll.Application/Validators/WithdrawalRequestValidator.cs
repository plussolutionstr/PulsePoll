using FluentValidation;
using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Validators;

public class WithdrawalRequestValidator : AbstractValidator<WithdrawalRequestDto>
{
    public WithdrawalRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(10000);
        RuleFor(x => x.BankAccountId).NotEmpty();
    }
}
