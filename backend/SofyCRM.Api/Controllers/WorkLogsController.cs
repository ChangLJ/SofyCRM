using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/worklogs")]
public class WorkLogsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public WorkLogsController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] Guid? projectId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var q = _db.WorkLogs.AsNoTracking()
            .Include(w => w.Project).Include(w => w.User).AsQueryable();
        if (projectId is not null) q = q.Where(w => w.ProjectId == projectId);
        if (from is not null)      q = q.Where(w => w.WorkDate >= from.Value);
        if (to   is not null)      q = q.Where(w => w.WorkDate <= to.Value);
        if (_me.IsSales)   q = q.Where(_ => false);   // Sales 不可看工時
        if (_me.IsService) q = q.Where(w => w.UserId == _me.UserId);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(w => w.WorkDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrService)]
    public async Task<ActionResult<WorkLog>> Create([FromBody] WorkLog dto)
    {
        dto.Id = Guid.NewGuid();
        dto.UserId = _me.UserId ?? throw new InvalidOperationException();
        dto.CreatedAt = DateTime.UtcNow;
        _db.WorkLogs.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.AdminOrService)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var w = await _db.WorkLogs.FindAsync(id);
        if (w is null) return NotFound();
        if (_me.IsService && w.UserId != _me.UserId) return Forbid();
        _db.WorkLogs.Remove(w);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
