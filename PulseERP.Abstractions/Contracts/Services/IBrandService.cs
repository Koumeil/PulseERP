using PulseERP.Abstractions.Common.DTOs.Brands.Commands;
using PulseERP.Abstractions.Common.DTOs.Brands.Models;
using PulseERP.Abstractions.Common.Pagination;

namespace PulseERP.Application.Interfaces;

public interface IBrandService
{
    Task<PagedResult<BrandSummary>> GetAllBrandsAsync(PaginationParams pagination);
    Task<BrandSummary> FindBrandByIdAsync(Guid id);
    Task<BrandSummary> FindBrandByNameAsync(string name);
    Task<BrandSummary> CreateBrandAsync(CreateBrandCommand command);
    Task<BrandSummary> UpdateBrandAsync(Guid id, UpdateBrandCommand command);
    Task DeleteBrandAsync(Guid id);
}
