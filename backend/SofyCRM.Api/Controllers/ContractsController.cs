using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/contracts")]
public class ContractsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ContractsController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<ActionResult<object>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = _db.Contracts.AsNoTracking().Include(c => c.Customer);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(c => c.EndDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Contract>> Get(Guid id)
    {
        var c = await _db.Contracts.AsNoTracking().Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Contract>> Create([FromBody] Contract dto)
    {
        dto.Id = Guid.NewGuid();
        dto.CreatedAt = dto.UpdatedAt = DateTime.UtcNow;
        _db.Contracts.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Contract>> Update(Guid id, [FromBody] Contract dto)
    {
        var c = await _db.Contracts.FindAsync(id);
        if (c is null) return NotFound();
        c.ContractName = dto.ContractName;
        c.StartDate = dto.StartDate; c.EndDate = dto.EndDate;
        c.RenewalNoticeDays = dto.RenewalNoticeDays;
        c.FileUrl = dto.FileUrl; c.Notes = dto.Notes;
        c.CustomerId = dto.CustomerId;
        c.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var c = await _db.Contracts.FindAsync(id);
        if (c is null) return NotFound();
        _db.Contracts.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
