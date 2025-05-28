using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(Guid id);
    Task<Brand?> GetByNameAsync(string name);
    Task<PaginationResult<Brand>> GetAllAsync(PaginationParams paginationParams);
    Task<Brand> AddAsync(Brand brand);
    Task UpdateAsync(Brand brand);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> NameExistsAsync(string name);
}