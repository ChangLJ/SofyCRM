using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/v1/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuditLogsController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] string? module,
        [FromQuery] string? action,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var q = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(module)) q = q.Where(a => a.Module == module);
        if (!string.IsNullOrWhiteSpace(action)) q = q.Where(a => a.Action == action);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }
}
