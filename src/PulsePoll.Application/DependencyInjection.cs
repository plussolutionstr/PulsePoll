using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Services;
using PulsePoll.Application.Validators;

namespace PulsePoll.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISubjectScoreService, SubjectScoreService>();
        services.AddScoped<ISubjectTelemetryService, SubjectTelemetryService>();
        services.AddScoped<ISubjectScoreConfigService, SubjectScoreConfigService>();
        services.AddScoped<IRewardUnitConfigService, RewardUnitConfigService>();
        services.AddScoped<IReferralRewardConfigService, ReferralRewardConfigService>();
        services.AddScoped<IAppContentConfigService, AppContentConfigService>();
        services.AddScoped<ICommunicationAutomationConfigService, CommunicationAutomationConfigService>();
        services.AddScoped<IReferralRewardService, ReferralRewardService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IStoryService, StoryService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IMediaAssetService, MediaAssetService>();
        services.AddScoped<IAccessControlService, AccessControlService>();
        services.AddScoped<IAdminPermissionCacheService, AdminPermissionCacheService>();
        services.AddScoped<IMessageAutomationService, MessageAutomationService>();
        services.AddScoped<ISpecialDayCalendarService, SpecialDayCalendarService>();
        services.AddScoped<IAdminAssignmentService, AdminAssignmentService>();
        services.AddScoped<IPaymentBatchService, PaymentBatchService>();
        services.AddScoped<IBankService, BankService>();
        services.AddScoped<SubjectProjectHistoryService>();
        services.AddScoped<ReferralService>();

        // Validators
        services.AddScoped<IValidator<LoginDto>, LoginValidator>();
        services.AddScoped<IValidator<RegisterSubjectDto>, RegisterRequestValidator>();
        services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerValidator>();
        services.AddScoped<IValidator<UpdateCustomerDto>, UpdateCustomerValidator>();
        services.AddScoped<IValidator<CreateProjectDto>, CreateProjectValidator>();
        services.AddScoped<IValidator<UpdateProjectDto>, UpdateProjectValidator>();
        services.AddScoped<IValidator<WithdrawalRequestDto>, WithdrawalRequestValidator>();
        services.AddScoped<IValidator<AddBankAccountDto>, AddBankAccountValidator>();

        return services;
    }
}
