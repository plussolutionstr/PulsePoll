using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByCodeAsync(string code);
    Task<List<Customer>> GetAllAsync();
    Task<List<Customer>> GetPagedAsync(int skip, int take);
    Task<int> CountAsync();
    Task<bool> ExistsByCodeAsync(string code);
    Task<bool> ExistsByTaxNumberAsync(string taxNumber);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
}
