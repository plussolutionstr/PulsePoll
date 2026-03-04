using FluentValidation;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Models;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class CustomerService(
    ICustomerRepository repository,
    IStorageService storage,
    IValidator<CreateCustomerDto> createValidator,
    IValidator<UpdateCustomerDto> updateValidator) : ICustomerService
{
    private const string BucketName = "customers";
    public async Task<List<CustomerDto>> GetAllAsync()
    {
        var customers = await repository.GetAllAsync();
        var dtos = new List<CustomerDto>(customers.Count);
        foreach (var c in customers)
            dtos.Add(await ToDtoAsync(c));
        return dtos;
    }

    public async Task<PagedResult<CustomerDto>> GetAllPagedAsync(int page, int pageSize)
    {
        var total     = await repository.CountAsync();
        var customers = await repository.GetPagedAsync(skip: (page - 1) * pageSize, take: pageSize);
        var dtos = new List<CustomerDto>(customers.Count);
        foreach (var c in customers)
            dtos.Add(await ToDtoAsync(c));
        return new PagedResult<CustomerDto>(dtos, total, page, pageSize);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await repository.GetByIdAsync(id);
        return customer is null ? null : await ToDtoAsync(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, int adminId)
    {
        await createValidator.ValidateAndThrowAsync(dto);

        if (await repository.ExistsByCodeAsync(dto.Code))
            throw new BusinessException("CUSTOMER_CODE_EXISTS", $"'{dto.Code}' kodlu müşteri zaten mevcut.");

        if (await repository.ExistsByTaxNumberAsync(dto.TaxNumber))
            throw new BusinessException("CUSTOMER_TAX_NUMBER_EXISTS", "Bu vergi numarasına ait müşteri zaten mevcut.");

        string? logoObjectName = null;
        if (dto.LogoStream is not null && dto.LogoFileName is not null)
        {
            logoObjectName = $"{Guid.NewGuid():N}{Path.GetExtension(dto.LogoFileName)}";
            await storage.UploadAsync(BucketName, logoObjectName, dto.LogoStream, GetContentType(dto.LogoFileName));
        }

        var customer = new Customer
        {
            Code          = dto.Code.ToUpperInvariant(),
            Title         = dto.Title,
            ShortName     = dto.ShortName,
            TaxNumber     = dto.TaxNumber,
            TaxOfficeId   = dto.TaxOfficeId,
            Phone1        = dto.Phone1,
            Phone2        = dto.Phone2,
            Mobile        = dto.Mobile,
            Email         = dto.Email,
            CityId        = dto.CityId,
            DistrictId    = dto.DistrictId,
            Address       = dto.Address,
            LogoUrl       = logoObjectName
        };
        customer.SetCreated(adminId);

        await repository.AddAsync(customer);

        var created = await repository.GetByIdAsync(customer.Id)
            ?? throw new NotFoundException("Müşteri");
        return await ToDtoAsync(created);
    }

    public async Task UpdateAsync(int id, UpdateCustomerDto dto, int adminId)
    {
        await updateValidator.ValidateAndThrowAsync(dto);

        var customer = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Müşteri");

        if (dto.LogoStream is not null && dto.LogoFileName is not null)
        {
            if (customer.LogoUrl is not null)
                await storage.DeleteAsync(BucketName, customer.LogoUrl);

            var logoObjectName = $"{Guid.NewGuid():N}{Path.GetExtension(dto.LogoFileName)}";
            await storage.UploadAsync(BucketName, logoObjectName, dto.LogoStream, GetContentType(dto.LogoFileName));
            customer.LogoUrl = logoObjectName;
        }

        customer.Title       = dto.Title;
        customer.ShortName   = dto.ShortName;
        customer.TaxNumber   = dto.TaxNumber;
        customer.TaxOfficeId = dto.TaxOfficeId;
        customer.Phone1      = dto.Phone1;
        customer.Phone2      = dto.Phone2;
        customer.Mobile      = dto.Mobile;
        customer.Email       = dto.Email;
        customer.CityId      = dto.CityId;
        customer.DistrictId  = dto.DistrictId;
        customer.Address     = dto.Address;
        customer.SetUpdated(adminId);

        await repository.UpdateAsync(customer);
    }

    public async Task DeleteAsync(int id, int adminId)
    {
        var customer = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Müşteri");

        customer.SetDeleted(adminId);
        await repository.DeleteAsync(customer);
    }

    private const int PresignedUrlExpirySeconds = 7 * 24 * 3600;

    private static string GetContentType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            _                 => "application/octet-stream"
        };

    private async Task<CustomerDto> ToDtoAsync(Customer c)
    {
        string? logoUrl = null;
        if (c.LogoUrl is not null)
            logoUrl = await storage.GetPresignedUrlAsync(BucketName, c.LogoUrl, PresignedUrlExpirySeconds);

        return new CustomerDto(
            c.Id,
            c.Code,
            c.Title,
            c.ShortName,
            c.TaxNumber,
            c.TaxOfficeId,
            c.TaxOffice?.Name  ?? string.Empty,
            c.Phone1,
            c.Phone2,
            c.Mobile,
            c.Email,
            c.CityId,
            c.City?.Name       ?? string.Empty,
            c.DistrictId,
            c.District?.Name   ?? string.Empty,
            c.Address,
            logoUrl,
            c.CreatedAt);
    }
}
