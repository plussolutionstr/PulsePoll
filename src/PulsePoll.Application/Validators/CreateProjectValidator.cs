using FluentValidation;
using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Validators;

public class CreateProjectValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Müşteri seçilmelidir.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Proje kodu zorunludur.")
            .MaximumLength(50)
            .Matches(@"^[A-Za-z0-9]+$").WithMessage("Proje kodu sadece harf ve rakam içerebilir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Proje adı zorunludur.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description != null);

        RuleFor(x => x.Category)
            .MaximumLength(100)
            .When(x => x.Category != null);

        RuleFor(x => x.ParticipantCount)
            .GreaterThan(0).WithMessage("Katılımcı sayısı 0'dan büyük olmalıdır.");

        RuleFor(x => x.TotalTargetCount)
            .GreaterThan(0).WithMessage("Toplam hedef sayısı 0'dan büyük olmalıdır.");

        RuleFor(x => x.ParticipantCount)
            .GreaterThanOrEqualTo(x => x.TotalTargetCount)
            .WithMessage("Katılımcı sayısı, hedef sayısından az olamaz.");

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Süre (gün) 0'dan büyük olmalıdır.");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Bütçe negatif olamaz.");

        RuleFor(x => x.Reward)
            .GreaterThanOrEqualTo(0).WithMessage("Ödül tutarı negatif olamaz.");

        RuleFor(x => x.ConsolationReward)
            .GreaterThanOrEqualTo(0).WithMessage("Teselli ödülü negatif olamaz.");

        RuleFor(x => x.SurveyUrl)
            .NotEmpty().WithMessage("Anket URL'si zorunludur.")
            .MaximumLength(2000)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Geçerli bir HTTP/HTTPS URL giriniz.");

        RuleFor(x => x.SubjectParameterName)
            .NotEmpty().WithMessage("Denek parametre adı zorunludur.")
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Parametre adı harf ile başlamalı, sadece harf/rakam/alt çizgi içermelidir.");

        RuleFor(x => x.EstimatedMinutes)
            .GreaterThan(0).WithMessage("Tahmini süre 0'dan büyük olmalıdır.");

        RuleFor(x => x.CustomerBriefing)
            .MaximumLength(2000)
            .When(x => x.CustomerBriefing != null);

        RuleFor(x => x.StartMessage)
            .NotEmpty().WithMessage("Başlangıç mesajı zorunludur.")
            .MaximumLength(500);

        RuleFor(x => x.CompletedMessage)
            .NotEmpty().WithMessage("Tamamlanma mesajı zorunludur.")
            .MaximumLength(500);

        RuleFor(x => x.DisqualifyMessage)
            .NotEmpty().WithMessage("Eleme mesajı zorunludur.")
            .MaximumLength(500);

        RuleFor(x => x.QuotaFullMessage)
            .NotEmpty().WithMessage("Kota dolu mesajı zorunludur.")
            .MaximumLength(500);

        RuleFor(x => x.ScreenOutMessage)
            .NotEmpty().WithMessage("Screen-out mesajı zorunludur.")
            .MaximumLength(500);
    }
}
