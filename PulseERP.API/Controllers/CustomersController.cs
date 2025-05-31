using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.API.Contracts;
using PulseERP.Application.Customers.Commands;
using PulseERP.Application.Customers.Models;
using PulseERP.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerSummary>>>> GetAll(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] CustomerFilter customerFilter
    )
    {
        var result = await _customerService.GetAllAsync(paginationParams, customerFilter);
        _logger.LogInformation(
            "API: Customers list fetched with params: {@CustomerParams}",
            customerFilter
        );
        return Ok(
            new ApiResponse<PagedResult<CustomerSummary>>(
                true,
                result,
                "Customers retrieved successfully"
            )
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDetails>>> GetById(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);
        return Ok(
            new ApiResponse<CustomerDetails>(true, result, "Customer retrieved successfully")
        );
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDetails>>> Create(
        [FromBody] CreateCustomerCommand request
    )
    {
        var result = await _customerService.CreateAsync(request);
        _logger.LogInformation("Customer created: {Customer}", request.Email);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            new ApiResponse<CustomerDetails>(true, result, "Customer created successfully")
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDetails>>> Update(
        Guid id,
        [FromBody] UpdateCustomerCommand request
    )
    {
        var result = await _customerService.UpdateAsync(id, request);
        _logger.LogInformation("Customer updated: {CustomerId}", id);
        return Ok(new ApiResponse<CustomerDetails>(true, result, "Customer updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _customerService.DeleteAsync(id);
        _logger.LogInformation("Customer deleted (soft): {CustomerId}", id);
        return NoContent();
    }

    [HttpPost("{customerId:guid}/assign/{userId:guid}")]
    public async Task<IActionResult> AssignToUser(Guid customerId, Guid userId)
    {
        await _customerService.AssignToUserAsync(customerId, userId);
        _logger.LogInformation(
            "Customer assigned to user: {CustomerId} -> {UserId}",
            customerId,
            userId
        );
        return NoContent();
    }

    [HttpPost("{customerId:guid}/interact")]
    public async Task<IActionResult> RecordInteraction(Guid customerId, [FromBody] string note)
    {
        await _customerService.RecordInteractionAsync(customerId, note);
        _logger.LogInformation("Customer interaction recorded: {CustomerId}", customerId);
        return NoContent();
    }
}
