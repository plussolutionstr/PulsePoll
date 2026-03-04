using Microsoft.Extensions.Logging;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Infrastructure.Notifications;

public class MockSmsService(
    ISmsLogRepository smsLogRepository,
    ILogger<MockSmsService> logger) : ISmsService
{
    private const string FixedOtp = "123456";

    public async Task<string> SendOtpAsync(string phoneNumber)
    {
        logger.LogInformation("[MOCK SMS] {PhoneNumber} → OTP: {Otp}", phoneNumber, FixedOtp);

        var log = new SmsLog
        {
            PhoneNumber = phoneNumber,
            Message = $"OTP: {FixedOtp}",
            Source = SmsSource.Otp,
            DeliveryStatus = DeliveryStatus.Sent
        };
        log.SetCreated(0);
        await smsLogRepository.AddAsync(log);

        return FixedOtp;
    }

    public Task SendAsync(string phoneNumber, string message,
        int? subjectId = null, int? sentByAdminId = null)
    {
        logger.LogInformation("[MOCK SMS] {PhoneNumber} → {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
