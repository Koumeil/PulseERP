using Microsoft.AspNetCore.Mvc;
using PulseERP.API.DTOs.Products;
using PulseERP.Application.DTOs.Products;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Filter;

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
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Quantity
        );

        var result = await _productService.CreateAsync(command);

        if (result.IsFailure)
        {
            _logger.LogWarning($"Create product failed: {result.Error}");
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value },
            new { productId = result.Value }
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllAsync();

        if (result.IsFailure)
            return StatusCode(500, result.Error);

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Quantity
        );

        var result = await _productService.UpdateAsync(id, command);

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

    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] ProductFilterRequest filter)
    {
        var result = await _productService.FilterAsync(filter);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
