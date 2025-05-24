using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PulseERP.API.DTOs.Customers;
using PulseERP.Application.DTOs.Customers;
using PulseERP.Application.Interfaces;

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

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);
        if (result.IsFailure)
            return NotFound(result.Error);

        var response = _mapper.Map<CustomerResponse>(result.Value);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerRequest request)
    {
        var command = _mapper.Map<CreateCustomerCommand>(request);
        var result = await _customerService.CreateAsync(command);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request)
    {
        var command = _mapper.Map<UpdateCustomerCommand>(request) with { Id = id };
        var result = await _customerService.UpdateAsync(id, command);

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
