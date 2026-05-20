using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/opportunities")]
public class OpportunitiesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public OpportunitiesController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    private IQueryable<Opportunity> Scoped()
    {
        var q = _db.Opportunities.AsNoTracking()
            .Include(o => o.Customer).Include(o => o.OwnerUser).AsQueryable();
        if (_me.IsAdmin)   return q;
        if (_me.IsSales)   return q.Where(o => o.OwnerUserId == _me.UserId);
        return q.Where(_ => false);
    }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] OpportunityStatus? status,
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = Scoped();
        if (status is not null) q = q.Where(o => o.Status == status);
        if (customerId is not null) q = q.Where(o => o.CustomerId == customerId);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Opportunity>> Get(Guid id)
    {
        var o = await Scoped().FirstOrDefaultAsync(x => x.Id == id);
        if (o is null) return NotFound();
        return Ok(o);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Opportunity>> Create([FromBody] Opportunity dto)
    {
        dto.Id = Guid.NewGuid();
        dto.OwnerUserId = _me.IsSales ? _me.UserId!.Value : (dto.OwnerUserId == Guid.Empty ? _me.UserId!.Value : dto.OwnerUserId);
        dto.CreatedAt = dto.UpdatedAt = DateTime.UtcNow;
        _db.Opportunities.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<Opportunity>> Update(Guid id, [FromBody] Opportunity dto)
    {
        var o = await _db.Opportunities.FindAsync(id);
        if (o is null) return NotFound();
        if (_me.IsSales && o.OwnerUserId != _me.UserId) return Forbid();

        o.Title  = dto.Title;
        o.Amount = dto.Amount;
        o.Status = dto.Status;
        o.ExpectedCloseDate = dto.ExpectedCloseDate;
        o.Description = dto.Description;
        o.CustomerId  = dto.CustomerId;
        o.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(o);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var o = await _db.Opportunities.FindAsync(id);
        if (o is null) return NotFound();
        _db.Opportunities.Remove(o);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
