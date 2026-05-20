using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/followups")]
public class FollowupsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public FollowupsController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = _db.CustomerFollowups.AsNoTracking()
            .Include(f => f.Customer).Include(f => f.User).AsQueryable();
        if (customerId is not null) q = q.Where(f => f.CustomerId == customerId);
        if (_me.IsSales)   q = q.Where(f => f.UserId == _me.UserId);
        if (_me.IsService) q = q.Where(f => false); // followup 屬於 sales 模組

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<ActionResult<CustomerFollowup>> Create([FromBody] CustomerFollowup dto)
    {
        dto.Id = Guid.NewGuid();
        dto.UserId = _me.UserId ?? throw new InvalidOperationException();
        dto.CreatedAt = DateTime.UtcNow;
        _db.CustomerFollowups.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.AdminOrSales)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var f = await _db.CustomerFollowups.FindAsync(id);
        if (f is null) return NotFound();
        if (_me.IsSales && f.UserId != _me.UserId) return Forbid();
        _db.CustomerFollowups.Remove(f);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
