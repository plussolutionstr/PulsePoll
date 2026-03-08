using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class BankService(IBankRepository bankRepository) : IBankService
{
    public Task<List<Bank>> GetAllAsync()
        => bankRepository.GetAllOrderedAsync();

    public async Task ToggleActiveAsync(int bankId)
    {
        var bank = await bankRepository.GetByIdAsync(bankId)
            ?? throw new NotFoundException("Banka");

        bank.IsActive = !bank.IsActive;
        await bankRepository.UpdateAsync(bank);
    }

    public async Task CreateOrUpdateAsync(int id, string name, string? code, string? bankCode, bool isActive, int? thumbnailMediaAssetId, int? logoMediaAssetId)
    {
        var normalizedName = name.Trim();
        var normalizedCode = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
        var normalizedBankCode = string.IsNullOrWhiteSpace(bankCode) ? null : bankCode.Trim();

        if (await bankRepository.ExistsByNameAsync(normalizedName, excludeId: id))
            throw new BusinessException("DUPLICATE_BANK", "Aynı isimde bir banka zaten mevcut.");

        if (!string.IsNullOrWhiteSpace(normalizedBankCode))
        {
            if (normalizedBankCode.Length != 5 || !normalizedBankCode.All(char.IsDigit))
                throw new BusinessException("INVALID_BANK_CODE", "Banka kodu 5 haneli sayısal bir değer olmalıdır.");

            if (await bankRepository.ExistsByBankCodeAsync(normalizedBankCode, excludeId: id))
                throw new BusinessException("DUPLICATE_BANK_CODE", "Aynı banka koduna sahip başka bir banka zaten mevcut.");
        }

        Bank entity;
        if (id > 0)
        {
            entity = await bankRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Banka");
        }
        else
        {
            entity = new Bank();
            await bankRepository.AddAsync(entity);
        }

        entity.Name = normalizedName;
        entity.Code = normalizedCode;
        entity.BankCode = normalizedBankCode;
        entity.IsActive = isActive;
        entity.ThumbnailMediaAssetId = thumbnailMediaAssetId;
        entity.LogoMediaAssetId = logoMediaAssetId;

        await bankRepository.UpdateAsync(entity);
    }
}
