using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Contracts.Interfaces.Services;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;

    public CustomersController(ICustomerService customerService, IMapper mapper)
    {
        _customerService = customerService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _customerService.GetAllAsync();
        if (result.IsFailure)
            return StatusCode(500, result.Error);

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerRequest request)
    {
        var result = await _customerService.CreateAsync(request);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Data }, null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request)
    {
        var result = await _customerService.UpdateAsync(id, request);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _customerService.DeleteAsync(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        return NoContent();
    }
}
