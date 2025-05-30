using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos;
using PulseERP.Application.Dtos.Customer;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Pagination;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService) =>
        _customerService = customerService;

    // GET api/customers?pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginationResult<CustomerDto>>>> GetAll(
        [FromQuery] PaginationParams paginationParams
    )
    {
        var result = await _customerService.GetAllAsync(paginationParams);
        return Ok(
            new ApiResponse<PaginationResult<CustomerDto>>(
                Success: true,
                Data: result,
                Message: "Customers retrieved successfully"
            )
        );
    }

    // GET api/customers/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(Guid id)
    {
        // Si le client n'existe pas, ton service lève NotFoundException → middleware renvoie 404
        var result = await _customerService.GetByIdAsync(id);
        return Ok(
            new ApiResponse<CustomerDto>(
                Success: true,
                Data: result.Data,
                Message: "Customer retrieved successfully"
            )
        );
    }

    // POST api/customers
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create(
        [FromBody] CreateCustomerRequest request
    )
    {
        var result = await _customerService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            new ApiResponse<CustomerDto>(
                Success: true,
                Data: result.Data,
                Message: "Customer created successfully"
            )
        );
    }

    // PUT api/customers/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(
        Guid id,
        [FromBody] UpdateCustomerRequest request
    )
    {
        var result = await _customerService.UpdateAsync(id, request);
        return Ok(
            new ApiResponse<CustomerDto>(
                Success: true,
                Data: result.Data,
                Message: "Customer updated successfully"
            )
        );
    }

    // DELETE api/customers/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _customerService.DeleteAsync(id);
        return NoContent();
    }

    // POST api/customers/{customerId}/assign/{userId}
    [HttpPost("{customerId:guid}/assign/{userId:guid}")]
    public async Task<IActionResult> AssignToUser(Guid customerId, Guid userId)
    {
        await _customerService.AssignToUserAsync(customerId, userId);
        return NoContent();
    }

    // POST api/customers/{customerId}/interact
    [HttpPost("{customerId:guid}/interact")]
    public async Task<IActionResult> RecordInteraction(Guid customerId, [FromBody] string note)
    {
        await _customerService.RecordInteractionAsync(customerId, note);
        return NoContent();
    }
}
