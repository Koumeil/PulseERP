using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Domain.Pagination;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginationResult<CustomerDto>>>> GetAll(
        [FromQuery] PaginationParams paginationParams
    )
    {
        var result = await _customerService.GetAllAsync(paginationParams);

        var response = new ApiResponse<PaginationResult<CustomerDto>>(
            Success: true,
            Data: result,
            Message: "Customers retrieved successfully"
        );

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);

        // Pas besoin de gérer result.IsFailure car une exception NotFoundException sera levée
        var response = new ApiResponse<CustomerDto>(
            Success: true,
            Data: result,
            Message: "Customer retrieved successfully"
        );

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create(CreateCustomerRequest request)
    {
        var result = await _customerService.CreateAsync(request);

        var response = new ApiResponse<CustomerDto>(
            Success: true,
            Data: result,
            Message: "Customer created successfully"
        );

        return CreatedAtAction(nameof(GetById), new { id = result }, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(
        Guid id,
        UpdateCustomerRequest request
    )
    {
        var result = await _customerService.UpdateAsync(id, request);

        var response = new ApiResponse<CustomerDto>(
            Success: true,
            Data: result,
            Message: "Customer updated successfully"
        );

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id)
    {
        await _customerService.DeactivateAsync(id);

        var response = new ApiResponse<object>(
            Success: true,
            Data: null,
            Message: "Customer deactivated successfully"
        );

        return Ok(response);
    }
}
