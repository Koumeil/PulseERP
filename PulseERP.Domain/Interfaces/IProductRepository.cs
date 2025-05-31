using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Interfaces;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetAllAsync(
        PaginationParams paginationParams,
        ProductFilter productFilter
    );
    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<int> SaveChangesAsync();
}
