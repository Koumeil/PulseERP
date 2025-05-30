using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos;
using PulseERP.Application.Dtos.Brand;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Pagination;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService) => _brandService = brandService;

    // GET api/brands?pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginationResult<BrandDto>>>> GetAll(
        [FromQuery] PaginationParams paginationParams
    )
    {
        var result = await _brandService.GetAllAsync(paginationParams);
        return Ok(
            new ApiResponse<PaginationResult<BrandDto>>(
                Success: true,
                Data: result,
                Message: "Brands retrieved successfully"
            )
        );
    }

    // GET api/brands/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> GetById(Guid id)
    {
        var result = await _brandService.GetByIdAsync(id);
        return Ok(
            new ApiResponse<BrandDto>(
                Success: true,
                Data: result.Data,
                Message: "Brand retrieved successfully"
            )
        );
    }

    // POST api/brands
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BrandDto>>> Create(
        [FromBody] CreateBrandRequest request
    )
    {
        var result = await _brandService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            new ApiResponse<BrandDto>(
                Success: true,
                Data: result.Data,
                Message: "Brand created successfully"
            )
        );
    }

    // PUT api/brands/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> Update(
        Guid id,
        [FromBody] UpdateBrandRequest request
    )
    {
        var result = await _brandService.UpdateAsync(id, request);
        return Ok(
            new ApiResponse<BrandDto>(
                Success: true,
                Data: result.Data,
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
