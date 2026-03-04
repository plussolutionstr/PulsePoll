namespace PulsePoll.Application.Interfaces;

public interface ISmsService
{
    /// <summary>
    /// OTP gönderir ve gönderilen kodu döner.
    /// Mock implementasyonu her zaman "123456" döner.
    /// </summary>
    Task<string> SendOtpAsync(string phoneNumber);

    /// <summary>
    /// Belirtilen numaraya serbest metin SMS gönderir ve kaydeder.
    /// </summary>
    Task SendAsync(string phoneNumber, string message,
        int? subjectId = null, int? sentByAdminId = null);
}
