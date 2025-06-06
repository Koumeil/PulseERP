using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Common.DTOs.Brands.Commands;
using PulseERP.Abstractions.Common.DTOs.Brands.Models;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.API.Contracts;
using PulseERP.Application.Interfaces;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService) => _brandService = brandService;

    // GET /api/brands
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BrandSummary>>>> GetAll(
        [FromQuery] PaginationParams paginationParams
    )
    {
        var result = await _brandService.GetAllBrandsAsync(paginationParams);
        return Ok(
            new ApiResponse<PagedResult<BrandSummary>>(
                true,
                result,
                "Brands retrieved successfully"
            )
        );
    }

    // GET /api/brands/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BrandSummary>>> GetById(Guid id)
    {
        var result = await _brandService.FindBrandByIdAsync(id);
        return Ok(new ApiResponse<BrandSummary>(true, result, "Brand retrieved successfully"));
    }

    // POST /api/brands
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BrandSummary>>> Create(
        [FromBody] CreateBrandCommand request
    )
    {
        var result = await _brandService.CreateBrandAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            new ApiResponse<BrandSummary>(true, result, "Brand created successfully")
        );
    }

    // PUT /api/brands/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BrandSummary>>> Update(
        Guid id,
        [FromBody] UpdateBrandCommand request
    )
    {
        var result = await _brandService.UpdateBrandAsync(id, request);
        return Ok(new ApiResponse<BrandSummary>(true, result, "Brand updated successfully"));
    }

    // DELETE /api/brands/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _brandService.DeleteBrandAsync(id);
        return NoContent();
    }
}
