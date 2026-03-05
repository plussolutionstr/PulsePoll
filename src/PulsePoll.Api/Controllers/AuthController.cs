using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Extensions;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, ILookupService lookupService, IWalletService walletService) : ControllerBase
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

    [HttpGet("register/cities")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> GetCities()
    {
        var cities = await lookupService.GetCitiesAsync();
        return this.OkResponse(cities.Select(c => new { c.Id, c.Name }));
    }

    [HttpGet("register/cities/{cityId:int}/districts")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> GetDistricts(int cityId)
    {
        var districts = await lookupService.GetDistrictsByCityIdAsync(cityId);
        return this.OkResponse(districts.Select(d => new { d.Id, d.Name }));
    }

    [HttpGet("register/professions")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> GetProfessions()
    {
        var professions = await lookupService.GetProfessionsAsync();
        return this.OkResponse(professions.Select(p => new { p.Id, p.Name }));
    }

    [HttpGet("register/education-levels")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> GetEducationLevels()
    {
        var levels = await lookupService.GetEducationLevelsAsync();
        return this.OkResponse(levels.Select(e => new { e.Id, e.Name }));
    }

    [HttpGet("register/bank-options")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> GetBankOptions()
    {
        var banks = await walletService.GetAvailableBanksAsync();
        return this.OkResponse(banks);
    }

    [HttpPost("send-password-reset-otp")]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> SendPasswordResetOtp([FromBody] SendPasswordResetOtpDto dto)
    {
        await authService.SendPasswordResetOtpAsync(dto.PhoneNumber);
        return this.NoContentResponse();
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await authService.ResetPasswordAsync(dto.PhoneNumber, dto.Otp, dto.NewPassword);
        return this.NoContentResponse();
    }
}
