using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Extensions;
using PulsePoll.Api.Models;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Models;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Api.Controllers.Admin;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/withdrawals")]
public class WithdrawalController(
    IWalletService walletService,
    IWithdrawalRequestRepository withdrawalRequestRepository) : ControllerBase
{
    private int AdminId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetPending([FromQuery] PaginationQuery query, [FromQuery] string status = "Pending")
    {
        var approvalStatus = Enum.TryParse<ApprovalStatus>(status, true, out var parsed)
            ? parsed
            : ApprovalStatus.Pending;

        var skip = (query.NormalizedPage - 1) * query.NormalizedPageSize;
        var (items, total) = await withdrawalRequestRepository.GetPagedAsync(approvalStatus, skip, query.NormalizedPageSize);

        var dtos = items.Select(r => new WithdrawalRequestAdminDto(
            r.Id,
            r.SubjectId,
            r.Subject.FullName,
            r.BankAccountId,
            r.BankAccount.BankName,
            r.BankAccount.IbanLast4,
            r.BankAccount.IbanEncrypted ?? r.BankAccount.IbanLast4,
            r.Amount,
            r.AmountTry,
            r.UnitCode,
            r.UnitLabel,
            r.UnitTryMultiplier,
            r.Status.ToString(),
            r.CreatedAt,
            r.RejectionReason,
            r.ProcessedAt)).ToList();

        var result = new PagedResult<WithdrawalRequestAdminDto>(dtos, total, query.NormalizedPage, query.NormalizedPageSize);
        return this.OkPagedResponse(result);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        await walletService.ApproveWithdrawalAsync(id, AdminId);
        return this.NoContentResponse();
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectWithdrawalDto dto)
    {
        await walletService.RejectWithdrawalAsync(id, dto.Reason, AdminId);
        return this.NoContentResponse();
    }
}
