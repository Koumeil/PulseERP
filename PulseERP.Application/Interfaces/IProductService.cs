using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Products.Commands;
using PulseERP.Application.Products.Models;

namespace PulseERP.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductSummary>> GetAllAsync(
        PaginationParams paginationParams,
        ProductFilter productFilter
    );
    Task<ProductDetails> GetByIdAsync(Guid id);
    Task<ProductDetails> CreateAsync(CreateProductCommand cmd);
    Task<ProductDetails> UpdateAsync(Guid id, UpdateProductCommand cmd);
    Task DeleteAsync(Guid id);
    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
}
