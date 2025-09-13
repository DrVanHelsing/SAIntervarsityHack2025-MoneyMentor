using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyMentor.ApiOrchestrator.Data;
using MoneyMentor.Shared.Models;

namespace MoneyMentor.ApiOrchestrator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll()
        => Ok(await _db.Categories.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<ActionResult<Category>> Create([FromBody] Category model)
    {
        if (model.CategoryId == 0)
        {
            // auto-generate next id if in-memory
            model.CategoryId = (_db.Categories.Any() ? _db.Categories.Max(c => c.CategoryId) : 0) + 1;
        }
        _db.Categories.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = model.CategoryId }, model);
    }
}
