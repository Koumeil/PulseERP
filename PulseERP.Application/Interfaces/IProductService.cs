using PulseERP.Application.DTOs.Products;
using PulseERP.Domain.Shared;

namespace PulseERP.Application.Interfaces;

public interface IProductService
{
    Task<Result<Guid>> CreateAsync(CreateProductCommand command);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync();
    Task<Result<ProductDto>> GetByIdAsync(Guid id);
    Task<Result> UpdateAsync(Guid id, UpdateProductCommand command);
}