using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;
using Twilio.Types;

namespace PulsePoll.Infrastructure.Notifications;

public class TwilioSmsService : ISmsService
{
    private readonly TwilioSettings _settings;
    private readonly ISmsLogRepository _smsLogRepository;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(
        IOptions<TwilioSettings> settings,
        ISmsLogRepository smsLogRepository,
        ILogger<TwilioSmsService> logger)
    {
        _settings = settings.Value;
        _smsLogRepository = smsLogRepository;
        _logger = logger;

        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    public async Task SendOtpAsync(string phoneNumber)
    {
        var verification = await VerificationResource.CreateAsync(
            to: phoneNumber,
            channel: "sms",
            locale: "tr",
            pathServiceSid: _settings.VerifyServiceSid);

        _logger.LogInformation("Twilio OTP gönderildi: {PhoneNumber}, Status: {Status}",
            phoneNumber, verification.Status);

        var log = new SmsLog
        {
            PhoneNumber = phoneNumber,
            Message = "OTP (Twilio Verify)",
            Source = SmsSource.Otp,
            DeliveryStatus = DeliveryStatus.Sent
        };
        log.SetCreated(0);
        await _smsLogRepository.AddAsync(log);
    }

    public async Task<bool> VerifyOtpAsync(string phoneNumber, string code)
    {
        var check = await VerificationCheckResource.CreateAsync(
            to: phoneNumber,
            code: code,
            pathServiceSid: _settings.VerifyServiceSid);

        _logger.LogInformation("Twilio OTP doğrulama: {PhoneNumber}, Status: {Status}",
            phoneNumber, check.Status);

        return check.Status == "approved";
    }

    public async Task SendAsync(string phoneNumber, string message,
        int? subjectId = null, int? sentByAdminId = null)
    {
        var msg = await MessageResource.CreateAsync(
            to: new PhoneNumber(phoneNumber),
            from: new PhoneNumber(_settings.FromNumber),
            body: message);

        _logger.LogInformation("Twilio SMS gönderildi: {PhoneNumber}, SID: {Sid}",
            phoneNumber, msg.Sid);

        var log = new SmsLog
        {
            PhoneNumber = phoneNumber,
            Message = message,
            SubjectId = subjectId,
            Source = sentByAdminId.HasValue ? SmsSource.AdminBulk : SmsSource.System,
            DeliveryStatus = DeliveryStatus.Sent
        };
        log.SetCreated(sentByAdminId ?? 0);
        await _smsLogRepository.AddAsync(log);
    }
}
