using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public ExpensesController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    private IQueryable<Expense> Scoped()
    {
        var q = _db.Expenses.AsNoTracking()
            .Include(e => e.User).Include(e => e.Customer).AsQueryable();
        if (_me.IsAdmin) return q;
        return q.Where(e => e.UserId == _me.UserId);
    }

    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] ExpenseStatus? status,
        [FromQuery] ExpenseCategory? category,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = Scoped();
        if (status   is not null) q = q.Where(e => e.Status == status);
        if (category is not null) q = q.Where(e => e.Category == category);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(e => e.ExpenseDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> Create([FromBody] Expense dto)
    {
        dto.Id = Guid.NewGuid();
        dto.UserId = _me.UserId ?? throw new InvalidOperationException();
        dto.Status = ExpenseStatus.Submitted;
        dto.CreatedAt = dto.UpdatedAt = DateTime.UtcNow;
        _db.Expenses.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var e = await _db.Expenses.FindAsync(id);
        if (e is null) return NotFound();
        e.Status = ExpenseStatus.Approved;
        e.ApprovedBy = _me.UserId;
        e.ApprovedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(e);
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Reject(Guid id)
    {
        var e = await _db.Expenses.FindAsync(id);
        if (e is null) return NotFound();
        e.Status = ExpenseStatus.Rejected;
        e.ApprovedBy = _me.UserId;
        e.ApprovedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(e);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var e = await _db.Expenses.FindAsync(id);
        if (e is null) return NotFound();
        if (!_me.IsAdmin && e.UserId != _me.UserId) return Forbid();
        _db.Expenses.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
