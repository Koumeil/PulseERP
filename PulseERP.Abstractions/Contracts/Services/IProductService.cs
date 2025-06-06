using PulseERP.Abstractions.Common.DTOs.Inventories.Models;
using PulseERP.Abstractions.Common.DTOs.Products.Commands;
using PulseERP.Abstractions.Common.DTOs.Products.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;

namespace PulseERP.Application.Interfaces;

public interface IProductService
{
    Task ActivateProductAsync(Guid id);
    Task<ProductDetails> ApplyDiscountAsync(Guid id, decimal percentage);
    Task ArchiveStaleProductsAsync(TimeSpan inactivityPeriod, int outOfStockDays);
    Task<ProductDetails> ChangeProductPriceAsync(Guid id, decimal newPrice);
    Task<ProductDetails> CreateProductAsync(CreateProductCommand cmd);
    Task DeactivateProductAsync(Guid id);
    Task DeleteProductAsync(Guid id);
    Task<PagedResult<ProductSummary>> GetAllProductsAsync(ProductFilter filter);
    Task<IReadOnlyCollection<InventoryMovementModel>> GetInventoryMovementsAsync(Guid id);
    Task<ProductDetails> GetProductByIdAsync(Guid id);
    Task<IReadOnlyCollection<ProductSummary>> GetProductsBelowThresholdAsync(int threshold = 5);
    Task<PagedResult<ProductSummary>> GetProductsByBrandAsync(
        string brandName,
        ProductFilter filter
    );
    Task<bool> IsProductLowStockAsync(Guid id, int threshold = 5);
    Task<bool> NeedsRestockingAsync(Guid id, int minThreshold);
    Task RestockProductAsync(Guid id, int quantity);
    Task RestoreProductAsync(Guid id);
    Task<ProductDetails> ReturnProductAsync(Guid id, int quantity);
    Task SellProductAsync(Guid id, int quantity);
    Task<ProductDetails> UpdateProductAsync(Guid id, UpdateProductCommand cmd);
}
