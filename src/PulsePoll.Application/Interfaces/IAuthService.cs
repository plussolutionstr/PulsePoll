using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IAuthService
{
    Task SendOtpAsync(string phoneNumber);
    Task<OtpVerifiedDto> VerifyOtpAsync(string phoneNumber, string otp);
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task QueueRegistrationAsync(RegisterSubjectDto dto);
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(int subjectId);
    Task SendPasswordResetOtpAsync(string phoneNumber);
    Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword);
}
