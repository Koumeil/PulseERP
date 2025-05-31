using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos;
using PulseERP.Application.Dtos.Product;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;

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
    public async Task<ActionResult<ApiResponse<PaginationResult<ProductDto>>>> GetAll(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] ProductParams productParams
    )
    {
        var result = await _productService.GetAllAsync(paginationParams, productParams);
        _logger.LogInformation("Retrieved products list: {Count} items", result.Items.Count);
        return Ok(
            new ApiResponse<PaginationResult<ProductDto>>(
                Success: true,
                Data: result,
                Message: "Products retrieved successfully"
            )
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        var response = new ApiResponse<ProductDto>(
            Success: true,
            Data: result.Data,
            Message: "Product retrieved successfully"
        );
        _logger.LogInformation("Retrieved product {ProductId}", id);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(
        [FromBody] CreateProductRequest request
    )
    {
        var result = await _productService.CreateAsync(request);

        var response = new ApiResponse<ProductDto>(
            Success: result.Success,
            Data: result.Data,
            Message: result.Success ? "Product created successfully" : result.ErrorMessage
        );

        if (!result.Success)
            _logger.LogWarning("Product creation failed: {ErrorMessage}", result.ErrorMessage);
        else
            _logger.LogInformation("Product created: {ProductName}", request.Name);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request
    )
    {
        var result = await _productService.UpdateAsync(id, request);

        var response = new ApiResponse<ProductDto>(
            Success: result.Success,
            Data: result.Data,
            Message: result.Success ? "Product updated successfully" : result.ErrorMessage
        );

        if (!result.Success)
            _logger.LogWarning(
                "Product update failed for {ProductId}: {ErrorMessage}",
                id,
                result.ErrorMessage
            );
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
