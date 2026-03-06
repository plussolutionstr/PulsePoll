using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IBankRepository
{
    Task<List<Bank>> GetAllOrderedAsync();
    Task<Bank?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int excludeId);
    Task AddAsync(Bank bank);
    Task UpdateAsync(Bank bank);
}
