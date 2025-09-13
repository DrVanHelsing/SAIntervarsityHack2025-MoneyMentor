using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyMentor.ApiOrchestrator.Data;
using MoneyMentor.Shared.Models;
using MoneyMentor.Shared.DTOs;

namespace MoneyMentor.ApiOrchestrator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ExpensesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetAll()
        => Ok(await _db.Expenses.AsNoTracking().ToListAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> Get(Guid id)
        => await _db.Expenses.FindAsync(id) is { } e ? Ok(e) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Expense>> Create([FromBody] ExpenseEntryDto dto)
    {
        var e = new Expense
        {
            ExpenseId = dto.ExpenseId == Guid.Empty ? Guid.NewGuid() : dto.ExpenseId,
            UserId = dto.UserId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "ZAR" : dto.Currency,
            Note = dto.Note,
            ExpenseDate = dto.ExpenseDate == default ? DateTime.UtcNow : dto.ExpenseDate
        };
        _db.Expenses.Add(e);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = e.ExpenseId }, e);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ExpenseEntryDto dto)
    {
        var existing = await _db.Expenses.FindAsync(id);
        if (existing is null) return NotFound();
        existing.CategoryId = dto.CategoryId;
        existing.Amount = dto.Amount;
        existing.Currency = dto.Currency;
        existing.Note = dto.Note;
        existing.ExpenseDate = dto.ExpenseDate;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _db.Expenses.FindAsync(id);
        if (existing is null) return NotFound();
        _db.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> Summary([FromQuery] string period = "weekly")
    {
        var now = DateTime.UtcNow;
        DateTime from = period.ToLowerInvariant() == "monthly"
            ? new DateTime(now.Year, now.Month, 1)
            : now.Date.AddDays(-7);
        var items = await _db.Expenses.Where(e => e.ExpenseDate >= from).ToListAsync();
        var total = items.Sum(e => e.Amount);
        var byCategory = items.GroupBy(e => e.CategoryId).Select(g => new { categoryId = g.Key, total = g.Sum(x => x.Amount) });
        return Ok(new { period, from, to = now, total, byCategory });
    }
}
