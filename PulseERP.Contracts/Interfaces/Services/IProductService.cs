using PulseERP.Contracts.Dtos.Products;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;

namespace PulseERP.Contracts.Interfaces.Services;

public interface IProductService
{
    Task<ServiceResult<PaginationResult<ProductDto>>>  GetAllAsync(ProductParams productParams);
    Task<ServiceResult<ProductDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<Guid>> CreateAsync(CreateProductRequest command);
    Task<ServiceResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest command);
    Task<ServiceResult> DeleteAsync(Guid id);
}
