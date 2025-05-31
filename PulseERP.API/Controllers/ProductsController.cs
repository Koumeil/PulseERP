using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.API.Contracts;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Products.Commands;
using PulseERP.Application.Products.Models;

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
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] ProductFilter productFilter
    )
    {
        var result = await _productService.GetAllAsync(paginationParams, productFilter);
        _logger.LogInformation("Retrieved products list: {Count} items", result.Items.Count);
        return Ok(
            new ApiResponse<PagedResult<ProductSummary>>(
                Success: true,
                Data: result,
                Message: "Products retrieved successfully"
            )
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        var response = new ApiResponse<ProductDetails>(
            Success: true,
            Data: result,
            Message: "Product retrieved successfully"
        );
        _logger.LogInformation("Retrieved product {ProductId}", id);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> Create(
        [FromBody] CreateProductCommand request
    )
    {
        var result = await _productService.CreateAsync(request);

        var response = new ApiResponse<ProductDetails>(
            Success: true,
            Data: result,
            Message: "Product created successfully"
        );

        if (result is null)
            _logger.LogWarning("Product creation failed");
        else
            _logger.LogInformation("Product created: {ProductName}", request.Name);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDetails>>> Update(
        Guid id,
        [FromBody] UpdateProductCommand request
    )
    {
        var result = await _productService.UpdateAsync(id, request);

        var response = new ApiResponse<ProductDetails>(
            Success: true,
            Data: result,
            Message: "Product updated successfully"
        );
        if (result is null)
            _logger.LogWarning("Product update failed for {ProductId}", id);
        else
            _logger.LogInformation("Product updated: {ProductId}", id);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _productService.DeleteAsync(id);

        var response = new ApiResponse<object>(
            Success: true,
            Data: null,
            Message: "Product deleted successfully"
        );

        _logger.LogInformation("Product deleted: {ProductId}", id);

        return Ok(response);
    }
}
