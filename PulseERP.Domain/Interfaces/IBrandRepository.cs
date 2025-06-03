using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Interfaces;

public interface IBrandRepository
{
    Task<PagedResult<Brand>> GetAllAsync(PaginationParams pagination);
    Task<Brand?> GetByIdAsync(Guid id);
    Task<Brand?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    Task AddAsync(Brand brand);
    Task UpdateAsync(Brand brand);
    Task DeleteAsync(Brand brand); // ou bien Deactivate en soft-delete
    Task<int> SaveChangesAsync();
}
