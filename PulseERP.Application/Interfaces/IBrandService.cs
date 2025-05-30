using PulseERP.Application.Common;
using PulseERP.Application.Dtos.Brand;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Interfaces;

public interface IBrandService
{
    Task<ServiceResult<BrandDto>> CreateAsync(CreateBrandRequest request);
    Task<ServiceResult<BrandDto>> UpdateAsync(Guid id, UpdateBrandRequest request);
    Task<ServiceResult<bool>> DeleteAsync(Guid id);
    Task<ServiceResult<BrandDto>> GetByIdAsync(Guid id);
    Task<PaginationResult<BrandDto>> GetAllAsync(PaginationParams pagination);
}
