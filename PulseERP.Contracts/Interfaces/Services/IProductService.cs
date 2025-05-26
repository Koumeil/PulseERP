using PulseERP.Contracts.Dtos.Products;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Domain.Filters.Products;

namespace PulseERP.Contracts.Interfaces.Services;

public interface IProductService
{
    Task<ServiceResult<Guid>> CreateAsync(CreateProductRequest command);
    Task<ServiceResult> DeleteAsync(Guid id);
    Task<ServiceResult<IReadOnlyList<ProductDto>>> GetAllAsync(ProductParams productParams);
    Task<ServiceResult<ProductDto>> GetByIdAsync(Guid id);
    Task<ServiceResult> UpdateAsync(Guid id, UpdateProductRequest command);
}
