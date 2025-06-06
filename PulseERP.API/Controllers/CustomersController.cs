using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Common.DTOs.Customers.Commands;
using PulseERP.Abstractions.Common.DTOs.Customers.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.API.Contracts;
using PulseERP.Application.Interfaces;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        ILogger<CustomersController> logger
    )
    {
        _customerService = customerService;
        _logger = logger;
    }

    // GET /api/customers
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerSummary>>>> GetAll(
        [FromQuery] CustomerFilter customerFilter
    )
    {
        var result = await _customerService.GetAllCustomersAsync(customerFilter);
        return Ok(
            new ApiResponse<PagedResult<CustomerSummary>>(
                true,
                result,
                "Customers retrieved successfully"
            )
        );
    }

    // GET /api/customers/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDetails>>> GetById(Guid id)
    {
        var result = await _customerService.FindCustomerByIdAsync(id);
        return Ok(
            new ApiResponse<CustomerDetails>(true, result, "Customer retrieved successfully")
        );
    }

    // POST /api/customers
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDetails>>> Create(
        [FromBody] CreateCustomerCommand request
    )
    {
        var result = await _customerService.CreateCustomerAsync(request);
        _logger.LogInformation("Customer created: {Customer}", request.Email);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            new ApiResponse<CustomerDetails>(true, result, "Customer created successfully")
        );
    }

    // PUT /api/customers/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDetails>>> Update(
        Guid id,
        [FromBody] UpdateCustomerCommand request
    )
    {
        var result = await _customerService.UpdateCustomerAsync(id, request);
        return Ok(new ApiResponse<CustomerDetails>(true, result, "Customer updated successfully"));
    }

    // DELETE /api/customers/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return NoContent();
    }

    // PATCH /api/customers/{id}/activate
    [HttpPatch("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id)
    {
        await _customerService.ActivateCustomerAsync(id);
        return Ok(new ApiResponse<object>(true, null, "Customer activated successfully"));
    }

    // PATCH /api/customers/{id}/restore
    [HttpPatch("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<object>>> Restore(Guid id)
    {
        await _customerService.RestoreCustomerAsync(id);
        return Ok(new ApiResponse<object>(true, null, "Customer restored successfully"));
    }

    // PATCH /api/customers/{id}/deactivate
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id)
    {
        await _customerService.DeactivateCustomerAsync(id);
        return Ok(new ApiResponse<object>(true, null, "Customer deactivated successfully"));
    }

    // PUT /api/customers/{customerId}/assign-to/{userId}
    [HttpPut("{customerId:guid}/assign-to/{userId:guid}")]
    public async Task<IActionResult> AssignToUser(Guid customerId, Guid userId)
    {
        await _customerService.AssignCustomerToUserAsync(customerId, userId);
        return NoContent();
    }

    // POST /api/customers/{customerId}/interactions
    [HttpPost("{customerId:guid}/interactions")]
    public async Task<IActionResult> RecordInteraction(Guid customerId, [FromBody] string note)
    {
        await _customerService.RecordCustomerInteractionAsync(customerId, note);
        return NoContent();
    }
}
