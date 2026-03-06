namespace PulsePoll.Application.Interfaces;

public interface ISmsService
{
    /// <summary>
    /// OTP gönderir (Twilio Verify veya Mock).
    /// </summary>
    Task SendOtpAsync(string phoneNumber);

    /// <summary>
    /// OTP doğrular. Başarısızsa false döner.
    /// </summary>
    Task<bool> VerifyOtpAsync(string phoneNumber, string code);

    /// <summary>
    /// Belirtilen numaraya serbest metin SMS gönderir ve kaydeder.
    /// </summary>
    Task SendAsync(string phoneNumber, string message,
        int? subjectId = null, int? sentByAdminId = null);
}
