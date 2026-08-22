using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBakery.Application.Services;
using NexusBakery.Domain.Entities;

namespace NexusBakery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MobilePhonesController : ControllerBase
{
    private readonly IMobilePhoneService _service;

    public MobilePhonesController(IMobilePhoneService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult> Get(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null, 
        [FromQuery] string? brand = null,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _service.GetPagedPhonesAsync(page, pageSize, search, brand, cancellationToken);
        
        return Ok(new {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MobilePhone>> GetById(string id, CancellationToken cancellationToken)
    {
        var phone = await _service.GetPhoneByIdAsync(id, cancellationToken);
        if (phone == null) return NotFound();
        return Ok(phone);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SystemAdmin")]
    public async Task<ActionResult<MobilePhone>> Create([FromBody] MobilePhone phone, CancellationToken cancellationToken)
    {
        var created = await _service.CreatePhoneAsync(phone, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
