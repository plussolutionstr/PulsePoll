using PulsePoll.Application.DTOs;
using PulsePoll.Application.Models;

namespace PulsePoll.Application.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync();
    Task<PagedResult<CustomerDto>> GetAllPagedAsync(int page, int pageSize);
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, int adminId);
    Task UpdateAsync(int id, UpdateCustomerDto dto, int adminId);
    Task DeleteAsync(int id, int adminId);
}
