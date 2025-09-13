using MoneyMentor.ApiOrchestrator.Data;
using MoneyMentor.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace MoneyMentor.ApiOrchestrator.Services;

public class ExpenseService
{
    private readonly AppDbContext _db;
    public ExpenseService(AppDbContext db) => _db = db;

    public async Task<Expense> AddAsync(Expense e)
    {
        _db.Expenses.Add(e);
        await _db.SaveChangesAsync();
        return e;
    }

    public Task<List<Expense>> GetUserExpensesAsync(Guid userId)
        => _db.Expenses.Where(x => x.UserId == userId).ToListAsync();
}
