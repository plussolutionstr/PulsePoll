using Microsoft.AspNetCore.Mvc;
using PulsePoll.Api.Extensions;
using PulsePoll.Api.Models;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Controllers;

// TODO: Admin auth eklenince [Authorize(Roles = "Admin")] eklenmeli
[ApiController]
[Route("api/customers")]
public class CustomerController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query)
    {
        var result = await customerService.GetAllPagedAsync(query.NormalizedPage, query.NormalizedPageSize);
        return this.OkPagedResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await customerService.GetByIdAsync(id);
        return this.OkResponse(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var created = await customerService.CreateAsync(dto, adminId: 0);
        return this.CreatedResponse(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        await customerService.UpdateAsync(id, dto, adminId: 0);
        return this.NoContentResponse();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await customerService.DeleteAsync(id, adminId: 0);
        return this.NoContentResponse();
    }
}
