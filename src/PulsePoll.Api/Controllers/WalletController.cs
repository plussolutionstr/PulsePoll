using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Extensions;
using PulsePoll.Api.Models;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/wallet")]
public class WalletController(IWalletService walletService) : ControllerBase
{
    private int SubjectId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var wallet = await walletService.GetBySubjectIdAsync(SubjectId);
        return this.OkResponse(wallet);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] PaginationQuery query)
    {
        var result = await walletService.GetTransactionsAsync(SubjectId, query.NormalizedPage, query.NormalizedPageSize);
        return this.OkPagedResponse(result);
    }

    [HttpGet("banks")]
    public async Task<IActionResult> GetBanks()
    {
        var accounts = await walletService.GetBankAccountsAsync(SubjectId);
        return this.OkResponse(accounts);
    }

    [HttpGet("bank-options")]
    public async Task<IActionResult> GetBankOptions()
    {
        var banks = await walletService.GetAvailableBanksAsync();
        return this.OkResponse(banks);
    }

    [HttpPost("banks")]
    [EnableRateLimiting("bank-add")]
    public async Task<IActionResult> AddBank([FromBody] AddBankAccountDto dto)
    {
        await walletService.AddBankAccountAsync(SubjectId, dto);
        return this.NoContentResponse();
    }

    [HttpDelete("banks/{id:int}")]
    public async Task<IActionResult> DeleteBank(int id)
    {
        await walletService.DeleteBankAccountAsync(SubjectId, id);
        return this.NoContentResponse();
    }

    [HttpPost("withdraw")]
    [EnableRateLimiting("withdrawal")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto dto)
    {
        await walletService.RequestWithdrawalAsync(SubjectId, dto);
        return this.AcceptedResponse();
    }
}
