using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos;
using PulseERP.Application.Dtos.Product;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginationResult<ProductDto>>>> GetAll(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] ProductParams productParams
    )
    {
        var result = await _productService.GetAllAsync(paginationParams, productParams);
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

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(
        [FromBody] CreateProductRequest request
    )
    {
        var result = await _productService.CreateAsync(request);

        var response = new ApiResponse<ProductDto>(
            Success: true,
            Data: result.Data,
            Message: "Product created successfully"
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request
    )
    {
        var result = await _productService.UpdateAsync(id, request);

        var response = new ApiResponse<ProductDto>(
            Success: true,
            Data: result.Data,
            Message: "Product updated successfully"
        );

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

        return Ok(response);
    }
}
