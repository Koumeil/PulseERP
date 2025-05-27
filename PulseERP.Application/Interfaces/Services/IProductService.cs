using PulseERP.Contracts.Dtos.Products;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;

namespace PulseERP.Application.Interfaces.Services;

public interface IProductService
{
    Task<PaginationResult<ProductDto>> GetAllAsync(ProductParams productParams);
    Task<ProductDto> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(CreateProductRequest command);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest command);
    Task DeleteAsync(Guid id);
}
