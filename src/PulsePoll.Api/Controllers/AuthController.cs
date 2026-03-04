using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("send-otp")]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
    {
        await authService.SendOtpAsync(dto.PhoneNumber);
        return this.NoContentResponse();
    }

    [HttpPost("verify-otp")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var result = await authService.VerifyOtpAsync(dto.PhoneNumber, dto.Otp);
        return this.OkResponse(result);
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth-register")]
    public async Task<IActionResult> Register([FromBody] RegisterSubjectDto dto)
    {
        await authService.QueueRegistrationAsync(dto);
        return this.AcceptedResponse();
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return this.OkResponse(result);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("token-refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        var result = await authService.RefreshTokenAsync(refreshToken);
        return this.OkResponse(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var subjectId = int.Parse(User.FindFirst("sub")!.Value);
        await authService.LogoutAsync(subjectId);
        return this.NoContentResponse();
    }
}
