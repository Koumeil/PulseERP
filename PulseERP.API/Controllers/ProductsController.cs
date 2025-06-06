using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Common.DTOs.Inventories.Models;
using PulseERP.Abstractions.Common.DTOs.Products.Commands;
using PulseERP.Abstractions.Common.DTOs.Products.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.API.Contracts;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductSummary>>>> GetAll(
        [FromQuery] ProductFilter productFilter
    )
    {
        var result = await _productService.GetAllProductsAsync(productFilter);
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
        var result = await _productService.GetProductByIdAsync(id);
        return Ok(new ApiResponse<ProductDetails>(true, result, "Product retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> Create(
        [FromBody] CreateProductCommand request
    )
    {
        var result = await _productService.CreateProductAsync(request);
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
        var result = await _productService.UpdateProductAsync(id, request);
        return Ok(new ApiResponse<ProductDetails>(true, result, "Product updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id)
    {
        await _productService.ActivateProductAsync(id);
        return Ok(new ApiResponse<object>(true, null, "Product activated successfully"));
    }

    [HttpPatch("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<object>>> Restore(Guid id)
    {
        await _productService.RestoreProductAsync(id);
        return Ok(new ApiResponse<object>(true, null, "Product restored successfully"));
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id)
    {
        await _productService.DeactivateProductAsync(id);
        return Ok(new ApiResponse<object>(true, null, "Product deactivated successfully"));
    }

    [HttpPost("{id:guid}/restock")]
    public async Task<ActionResult<ApiResponse<object>>> Restock(
        Guid id,
        [FromBody] RestockProductCommand request
    )
    {
        await _productService.RestockProductAsync(id, request.Quantity);
        return Ok(new ApiResponse<object>(true, null, "Product restocked successfully"));
    }

    [HttpPost("{id:guid}/sell")]
    public async Task<ActionResult<ApiResponse<object>>> Sell(
        Guid id,
        [FromBody] SellProductCommand request
    )
    {
        await _productService.SellProductAsync(id, request.Quantity);
        return Ok(new ApiResponse<object>(true, null, "Product sold successfully"));
    }

    [HttpPatch("{id:guid}/price")]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> ChangePrice(
        Guid id,
        [FromBody] ChangeProductPriceCommand request
    )
    {
        var result = await _productService.ChangeProductPriceAsync(id, request.NewPrice);
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
        var result = await _productService.ApplyDiscountAsync(id, request.Percentage);
        return Ok(new ApiResponse<ProductDetails>(true, result, "Discount applied successfully"));
    }

    [HttpGet("{id:guid}/is-low-stock")]
    public async Task<ActionResult<ApiResponse<bool>>> IsLowStock(
        Guid id,
        [FromQuery] int threshold = 5
    )
    {
        var result = await _productService.IsProductLowStockAsync(id, threshold);
        return Ok(new ApiResponse<bool>(true, result, "Low stock status retrieved successfully"));
    }

    [HttpGet("{id:guid}/needs-restock")]
    public async Task<ActionResult<ApiResponse<bool>>> NeedsRestock(
        Guid id,
        [FromQuery(Name = "minThreshold")] int minThreshold
    )
    {
        var result = await _productService.NeedsRestockingAsync(id, minThreshold);
        return Ok(
            new ApiResponse<bool>(true, result, "Needs restock status retrieved successfully")
        );
    }

    [HttpGet("low-stock-list")]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyCollection<ProductSummary>>>
    > GetLowStockList([FromQuery] int threshold = 5)
    {
        var result = await _productService.GetProductsBelowThresholdAsync(threshold);
        return Ok(
            new ApiResponse<IReadOnlyCollection<ProductSummary>>(
                true,
                result,
                "Low stock products retrieved successfully"
            )
        );
    }

    [HttpGet("{id:guid}/inventory-movements")]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyCollection<InventoryMovementModel>>>
    > GetInventoryMovements(Guid id)
    {
        var result = await _productService.GetInventoryMovementsAsync(id);
        return Ok(
            new ApiResponse<IReadOnlyCollection<InventoryMovementModel>>(
                true,
                result,
                "Inventory movements retrieved successfully"
            )
        );
    }

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> ReturnProduct(
        Guid id,
        [FromBody] ReturnProductCommand request
    )
    {
        var result = await _productService.ReturnProductAsync(id, request.Quantity);
        return Ok(
            new ApiResponse<ProductDetails>(true, result, "Product return processed successfully")
        );
    }

    [HttpPost("archive-stale")]
    public async Task<ActionResult<ApiResponse<object>>> ArchiveStaleProducts(
        [FromBody] ArchiveStaleProductsCommand request
    )
    {
        var inactivity = TimeSpan.FromDays(request.InactivityDays);
        await _productService.ArchiveStaleProductsAsync(inactivity, request.OutOfStockDays);
        return Ok(new ApiResponse<object>(true, null, "Stale products archived successfully"));
    }

    [HttpGet("by-brand")]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductSummary>>>> GetByBrand(
        [FromQuery] string brandName,
        [FromQuery] ProductFilter filter
    )
    {
        var result = await _productService.GetProductsByBrandAsync(brandName, filter);
        return Ok(
            new ApiResponse<PagedResult<ProductSummary>>(
                true,
                result,
                "Products by brand retrieved successfully"
            )
        );
    }
}


// using Microsoft.AspNetCore.Mvc;
// using PulseERP.Abstractions.Common.Filters;
// using PulseERP.Abstractions.Common.Pagination;
// using PulseERP.API.Contracts;
// using PulseERP.Application.Interfaces;
// using PulseERP.Application.Products.Commands;
// using PulseERP.Application.Products.Models;

// [ApiController]
// [Route("api/products")]
// public class ProductsController : ControllerBase
// {
//     private readonly IProductService _productService;
//     private readonly ILogger<ProductsController> _logger;

//     public ProductsController(IProductService productService, ILogger<ProductsController> logger)
//     {
//         _productService = productService;
//         _logger = logger;
//     }

//     // GET /api/products
//     [HttpGet]
//     public async Task<ActionResult<ApiResponse<PagedResult<ProductSummary>>>> GetAll(
//         [FromQuery] ProductFilter productFilter
//     )
//     {
//         var result = await _productService.GetAllProductsAsync(productFilter);
//         return Ok(
//             new ApiResponse<PagedResult<ProductSummary>>(
//                 true,
//                 result,
//                 "Products retrieved successfully"
//             )
//         );
//     }

//     // GET /api/products/{id}
//     [HttpGet("{id:guid}")]
//     public async Task<ActionResult<ApiResponse<ProductDetails>>> GetById(Guid id)
//     {
//         var result = await _productService.GetProductByIdAsync(id);
//         return Ok(new ApiResponse<ProductDetails>(true, result, "Product retrieved successfully"));
//     }

//     // POST /api/products
//     [HttpPost]
//     public async Task<ActionResult<ApiResponse<ProductDetails>>> Create(
//         [FromBody] CreateProductCommand request
//     )
//     {
//         var result = await _productService.CreateProductAsync(request);
//         var response = new ApiResponse<ProductDetails>(
//             true,
//             result,
//             "Product created successfully"
//         );

//         return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
//     }

//     // PUT /api/products/{id}
//     [HttpPut("{id:guid}")]
//     public async Task<ActionResult<ApiResponse<ProductDetails>>> Update(
//         Guid id,
//         [FromBody] UpdateProductCommand request
//     )
//     {
//         var result = await _productService.UpdateProductAsync(id, request);
//         return Ok(new ApiResponse<ProductDetails>(true, result, "Product updated successfully"));
//     }

//     // DELETE /api/products/{id}
//     [HttpDelete("{id:guid}")]
//     public async Task<IActionResult> Delete(Guid id)
//     {
//         await _productService.DeleteProductAsync(id);
//         return NoContent(); // standard REST response
//     }

//     // PATCH /api/products/{id}/activate
//     [HttpPatch("{id:guid}/activate")]
//     public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id)
//     {
//         await _productService.ActivateProductAsync(id);
//         return Ok(new ApiResponse<object>(true, null, "Product activated successfully"));
//     }

//     // PATCH /api/products/{id}/restore
//     [HttpPatch("{id:guid}/restore")]
//     public async Task<ActionResult<ApiResponse<object>>> Restore(Guid id)
//     {
//         await _productService.RestoreProductAsync(id);
//         return Ok(new ApiResponse<object>(true, null, "Product restored successfully"));
//     }

//     // PATCH /api/products/{id}/deactivate
//     [HttpPatch("{id:guid}/deactivate")]
//     public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id)
//     {
//         await _productService.DeactivateProductAsync(id);
//         return Ok(new ApiResponse<object>(true, null, "Product deactivated successfully"));
//     }
// }
