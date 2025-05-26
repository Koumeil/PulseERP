using Microsoft.AspNetCore.Mvc;
using PulseERP.Contracts.Dtos.Pagination;
using PulseERP.Contracts.Dtos.Products;
using PulseERP.Contracts.Interfaces.Services;
using PulseERP.Domain.Filters.Products;

namespace PulseERP.API.Controllers;

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request);

        if (result.IsFailure)
        {
            _logger.LogWarning($"Create product failed: {result.Error}");
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data },
            new { productId = result.Data }
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResult<ProductDto>>> GetAll(
       [FromQuery] ProductParams productParams
    )
    {
        var result = await _productService.GetAllAsync(productParams);

        if (result.IsFailure)
            return StatusCode(500, result.Error);

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _productService.UpdateAsync(id, request);

        if (result.IsFailure)
        {
            if (result.Error == "Product not found")
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);

        if (result.IsFailure)
        {
            if (result.Error == "Product not found")
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
