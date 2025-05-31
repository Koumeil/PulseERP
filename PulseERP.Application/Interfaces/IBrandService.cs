using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Brands.Commands;
using PulseERP.Application.Brands.Models;

namespace PulseERP.Application.Interfaces;

public interface IBrandService
{
    Task<BrandSummary> CreateAsync(CreateBrandCommand command);
    Task<BrandSummary> UpdateAsync(Guid id, UpdateBrandCommand command);
    Task DeleteAsync(Guid id);
    Task<BrandSummary> GetByIdAsync(Guid id);
    Task<PagedResult<BrandSummary>> GetAllAsync(PaginationParams pagination);
}
