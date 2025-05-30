using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface IBrandRepository
{
    Task<PaginationResult<Brand>> GetAllAsync(PaginationParams pagination);
    Task<Brand?> GetByIdAsync(Guid id);
    Task<Brand?> GetByNameAsync(string name);
    Task AddAsync(Brand brand);
    Task UpdateAsync(Brand brand);
    Task DeleteAsync(Brand brand); // ou bien Deactivate en soft-delete
    Task<int> SaveChangesAsync();
}
