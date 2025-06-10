namespace PulseERP.API.Controllers;


using Microsoft.AspNetCore.Mvc;
using Abstractions.Common.DTOs.Inventories.Models;
using Abstractions.Common.DTOs.Products.Commands;
using Abstractions.Common.DTOs.Products.Models;
using Abstractions.Common.Filters;
using Abstractions.Common.Pagination;
using Contracts;
using Application.Interfaces;

[ApiController]
[Route("api/products")]

public class ProductsController(IProductService productService)
    : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductSummary>>>> GetAll(
        [FromQuery] ProductFilter productFilter
    )
    {
        var result = await productService.GetAllProductsAsync(productFilter);
        return Ok(
            new ApiResponse<PagedResult<ProductSummary>>(
                true,
                result,
                "Products retrieved successfully"
            )
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> GetById(Guid id)
    {
        var result = await productService.GetProductByIdAsync(id);
        return Ok(new ApiResponse<ProductDetails>(true, result, "Product retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> Create(
        [FromBody] CreateProductCommand request
    )
    {
        var result = await productService.CreateProductAsync(request);
        var response = new ApiResponse<ProductDetails>(
            true,
            result,
            "Product created successfully"
        );
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> Update(
        Guid id,
        [FromBody] UpdateProductCommand request
    )
    {
        var result = await productService.UpdateProductAsync(id, request);
        return Ok(new ApiResponse<ProductDetails>(true, result, "Product updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await productService.DeleteProductAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id)
    {
        await productService.ActivateProductAsync(id);
        return Ok(new ApiResponse<object>(true, null, "Product activated successfully"));
    }

    [HttpPatch("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<object>>> Restore(Guid id)
    {
        await productService.RestoreProductAsync(id);
        return Ok(new ApiResponse<object>(true, null, "Product restored successfully"));
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id)
    {
        await productService.DeactivateProductAsync(id);
        return Ok(new ApiResponse<object>(true, null, "Product deactivated successfully"));
    }

    [HttpPost("{id:guid}/restock")]
    public async Task<ActionResult<ApiResponse<object>>> Restock(
        Guid id,
        [FromBody] RestockProductCommand request
    )
    {
        await productService.RestockProductAsync(id, request.Quantity);
        return Ok(new ApiResponse<object>(true, null, "Product restocked successfully"));
    }

    [HttpPost("{id:guid}/sell")]
    public async Task<ActionResult<ApiResponse<object>>> Sell(
        Guid id,
        [FromBody] SellProductCommand request
    )
    {
        await productService.SellProductAsync(id, request.Quantity);
        return Ok(new ApiResponse<object>(true, null, "Product sold successfully"));
    }

    [HttpPatch("{id:guid}/price")]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> ChangePrice(
        Guid id,
        [FromBody] ChangeProductPriceCommand request
    )
    {
        var result = await productService.ChangeProductPriceAsync(id, request.NewPrice);
        return Ok(
            new ApiResponse<ProductDetails>(true, result, "Product price changed successfully")
        );
    }

    [HttpPatch("{id:guid}/discount")]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> ApplyDiscount(
        Guid id,
        [FromBody] ApplyDiscountCommand request
    )
    {
        var result = await productService.ApplyDiscountAsync(id, request.Percentage);
        return Ok(new ApiResponse<ProductDetails>(true, result, "Discount applied successfully"));
    }

    [HttpGet("{id:guid}/is-low-stock")]
    public async Task<ActionResult<ApiResponse<bool>>> IsLowStock(
        Guid id,
        [FromQuery] int threshold = 5
    )
    {
        var result = await productService.IsProductLowStockAsync(id, threshold);
        return Ok(new ApiResponse<bool>(true, result, "Low stock status retrieved successfully"));
    }

    [HttpGet("{id:guid}/needs-restock")]
    public async Task<ActionResult<ApiResponse<bool>>> NeedsRestock(
        Guid id,
        [FromQuery(Name = "minThreshold")] int minThreshold
    )
    {
        var result = await productService.NeedsRestockingAsync(id, minThreshold);
        return Ok(
            new ApiResponse<bool>(true, result, "Needs restock status retrieved successfully")
        );
    }

    [HttpGet("low-stock-list")]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyCollection<ProductSummary>>>
    > GetLowStockList([FromQuery] int threshold = 5)
    {
        var result = await productService.GetProductsBelowThresholdAsync(threshold);
        return Ok(
            new ApiResponse<IReadOnlyCollection<ProductSummary>>(
                true,
                result,
                "Low stock products retrieved successfully"
            )
        );
    }

    [HttpPost("archive-stale")]
    public async Task<ActionResult<ApiResponse<object>>> ArchiveStaleProducts(
        [FromBody] ArchiveStaleProductsCommand request
    )
    {
        var inactivity = TimeSpan.FromDays(request.InactivityDays);
        await productService.ArchiveStaleProductsAsync(inactivity, request.OutOfStockDays);
        return Ok(new ApiResponse<object>(true, null, "Stale products archived successfully"));
    }

    [HttpGet("by-brand")]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductSummary>>>> GetByBrand(
        [FromQuery] string brandName,
        [FromQuery] ProductFilter filter
    )
    {
        var result = await productService.GetProductsByBrandAsync(brandName, filter);
        return Ok(
            new ApiResponse<PagedResult<ProductSummary>>(
                true,
                result,
                "Products by brand retrieved successfully"
            )
        );
    }
}
