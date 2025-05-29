using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;
using PulseERP.Shared.Dtos.Brands;

namespace PulseERP.Application.Interfaces;

public interface IBrandService
{
    Task<BrandDto?> GetByIdAsync(Guid id);
    Task<PaginationResult<BrandDto>> GetAllAsync(PaginationParams paginationParams);
    Task<BrandDto> CreateAsync(CreateBrandDto dto);
    Task UpdateAsync(UpdateBrandDto dto);
    Task DeleteAsync(Guid id);
    Task<Brand> GetOrCreateAsync(string brandName);
}
