using PulseERP.Application.Common;
using PulseERP.Application.Dtos.Product;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;

namespace PulseERP.Application.Interfaces;

public interface IProductService
{
    Task<PaginationResult<ProductDto>> GetAllAsync(
        PaginationParams pagination,
        ProductParams productParams
    );
    Task<ServiceResult<ProductDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<ProductDto>> CreateAsync(CreateProductRequest request);
    Task<ServiceResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request);
    Task<ServiceResult<bool>> DeleteAsync(Guid id);
    Task<ServiceResult> ActivateAsync(Guid id);
    Task<ServiceResult> DeactivateAsync(Guid id);
}
