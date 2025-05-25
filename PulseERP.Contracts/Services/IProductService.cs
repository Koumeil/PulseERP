using PulseERP.Contracts.Dtos.Products;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Domain.Filters.Products;

namespace PulseERP.Contracts.Services;

public interface IProductService
{
    Task<Result<Guid>> CreateAsync(CreateProductCommand command);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync(ProductParams productParams);
    Task<Result<ProductDto>> GetByIdAsync(Guid id);
    Task<Result> UpdateAsync(Guid id, UpdateProductCommand command);
}
