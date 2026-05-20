using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofyCRM.Api.Auth;
using SofyCRM.Api.Data;
using SofyCRM.Api.Entities;

namespace SofyCRM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _me;
    public DashboardController(AppDbContext db, ICurrentUser me) { _db = db; _me = me; }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> Summary()
    {
        var customerCount    = await _db.Customers.CountAsync();
        var openOpportunities= await _db.Opportunities.CountAsync(o => o.Status != OpportunityStatus.Won && o.Status != OpportunityStatus.Lost);
        var monthlyRevenue   = await _db.Invoices
            .Where(i => i.IssuedDate != null
                     && i.IssuedDate.Value.Year == DateTime.UtcNow.Year
                     && i.IssuedDate.Value.Month == DateTime.UtcNow.Month)
            .SumAsync(i => (decimal?)i.Amount) ?? 0m;
        var openTickets      = await _db.Tickets.CountAsync(t => t.Status != TicketStatus.Closed);
        var wonOpps          = await _db.Opportunities.CountAsync(o => o.Status == OpportunityStatus.Won);
        var lostOpps         = await _db.Opportunities.CountAsync(o => o.Status == OpportunityStatus.Lost);
        var winRate          = (wonOpps + lostOpps) == 0 ? 0 : Math.Round((double)wonOpps / (wonOpps + lostOpps) * 100, 1);

        var pipeline = await _db.Opportunities
            .GroupBy(o => o.Status)
            .Select(g => new { status = g.Key.ToString(), count = g.Count(), amount = g.Sum(o => o.Amount) })
            .ToListAsync();

        var revenueTrend = await _db.Invoices
            .Where(i => i.IssuedDate != null && i.IssuedDate.Value.Year == DateTime.UtcNow.Year)
            .GroupBy(i => i.IssuedDate!.Value.Month)
            .Select(g => new { month = g.Key, amount = g.Sum(x => x.Amount) })
            .OrderBy(x => x.month).ToListAsync();

        return Ok(new
        {
            customerCount,
            openOpportunities,
            monthlyRevenue,
            openTickets,
            winRate,
            pipeline,
            revenueTrend,
            generatedAt = DateTime.UtcNow,
        });
    }

    [HttpGet("my-followups")]
    public async Task<ActionResult<object>> MyFollowups()
    {
        var list = await _db.CustomerFollowups
            .Include(f => f.Customer)
            .Where(f => f.UserId == _me.UserId && f.NextFollowupDate != null)
            .OrderBy(f => f.NextFollowupDate)
            .Take(10)
            .ToListAsync();
        return Ok(list);
    }
}
