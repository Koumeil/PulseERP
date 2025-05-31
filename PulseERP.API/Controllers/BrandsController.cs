using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.API.Contracts;
using PulseERP.Application.Brands.Commands;
using PulseERP.Application.Brands.Models;
using PulseERP.Application.Interfaces;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService) => _brandService = brandService;

    // GET api/brands?pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BrandSummary>>>> GetAll(
        [FromQuery] PaginationParams paginationParams
    )
    {
        var result = await _brandService.GetAllAsync(paginationParams);
        return Ok(
            new ApiResponse<PagedResult<BrandSummary>>(
                Success: true,
                Data: result,
                Message: "Brands retrieved successfully"
            )
        );
    }

    // GET api/brands/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BrandSummary>>> GetById(Guid id)
    {
        var result = await _brandService.GetByIdAsync(id);
        return Ok(
            new ApiResponse<BrandSummary>(
                Success: true,
                Data: result,
                Message: "Brand retrieved successfully"
            )
        );
    }

    // POST api/brands
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BrandSummary>>> Create(
        [FromBody] CreateBrandCommand request
    )
    {
        var result = await _brandService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            new ApiResponse<BrandSummary>(
                Success: true,
                Data: result,
                Message: "Brand created successfully"
            )
        );
    }

    // PUT api/brands/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BrandSummary>>> Update(
        Guid id,
        [FromBody] UpdateBrandCommand request
    )
    {
        var result = await _brandService.UpdateAsync(id, request);
        return Ok(
            new ApiResponse<BrandSummary>(
                Success: true,
                Data: result,
                Message: "Brand updated successfully"
            )
        );
    }

    // DELETE api/brands/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _brandService.DeleteAsync(id);
        return NoContent();
    }
}
