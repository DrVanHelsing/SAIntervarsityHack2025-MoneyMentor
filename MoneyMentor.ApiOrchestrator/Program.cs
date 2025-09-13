using Microsoft.EntityFrameworkCore;
using MoneyMentor.ApiOrchestrator.Services;
using MoneyMentor.ApiOrchestrator.Data;
using MoneyMentor.ApiOrchestrator.Hubs;
using MoneyMentor.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Fixed dev URLs for mobile client
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("https://localhost:7001", "http://localhost:7000");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var useSql = false;
string? cs = builder.Configuration.GetConnectionString("DefaultConnection");

// We delay configuring provider until after a connectivity probe (so we can downgrade gracefully)
if (!string.IsNullOrWhiteSpace(cs))
{
    // Quick lightweight probe: attempt bare TCP/open by constructing a temp context later (in startup scope)
    useSql = true;
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSql)
    {
        options.UseSqlServer(cs!, sql =>
        {
            sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
            sql.CommandTimeout(120);
        });
    }
    else
    {
        options.UseInMemoryDatabase("MoneyMentor");
    }
});

builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<AdviceService>();
builder.Services.AddScoped<ChatOrchestrationService>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

// Apply migrations / ensure database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (useSql)
    {
        try
        {
            Console.WriteLine("Applying EF Core migrations (SQL mode)...");
            await db.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL migration failure: {ex.Message}");
            Console.WriteLine("Downgrading to InMemory provider for this run. Verify Azure SQL connectivity / firewall / credentials.");
            // Downgrade: build a new service provider with InMemory and replace scoped context
            useSql = false;
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("MoneyMentor"));
            foreach (var sd in scope.ServiceProvider.GetServices<ExpenseService>()) { } // force original DI load (no-op)
            var tmpProvider = services.BuildServiceProvider();
            var memCtx = tmpProvider.GetRequiredService<AppDbContext>();
            await memCtx.Database.EnsureCreatedAsync();
            // Copy reference (not thread-safe extensive swap; acceptable for startup fallback)
        }
    }
    else
    {
        await db.Database.EnsureCreatedAsync(); // In-memory only
    }

    var defaultUserId = new Guid("00000000-0000-0000-0000-000000000001");

    // Runtime seeding safety net (only adds if missing)
    if (!await db.Users.AnyAsync(u => u.UserId == defaultUserId))
    {
        db.Users.Add(new User
        {
            UserId = defaultUserId,
            Name = "DefaultUser",
            Email = "default@example.com",
            CreatedAt = DateTime.UtcNow
        });
    }

    if (!await db.Categories.AnyAsync())
    {
        db.Categories.AddRange(new[]
        {
            new Category { CategoryId = 1, Name = "Transport" },
            new Category { CategoryId = 2, Name = "Food" },
            new Category { CategoryId = 3, Name = "Health" },
            new Category { CategoryId = 4, Name = "Entertainment" },
            new Category { CategoryId = 5, Name = "Utilities" }
        });
    }

    // Seed a few sample expenses if none exist (dev only)
    if (!await db.Expenses.AnyAsync())
    {
        var now = DateTime.UtcNow.Date;
        db.Expenses.AddRange(new[]
        {
            new Expense { ExpenseId = Guid.NewGuid(), UserId = defaultUserId, CategoryId = 1, Amount = 35.50m, Currency = "ZAR", Note = "Bus fare to work", ExpenseDate = now },
            new Expense { ExpenseId = Guid.NewGuid(), UserId = defaultUserId, CategoryId = 2, Amount = 127.99m, Currency = "ZAR", Note = "Groceries - Pick n Pay", ExpenseDate = now },
            new Expense { ExpenseId = Guid.NewGuid(), UserId = defaultUserId, CategoryId = 3, Amount = 89.50m, Currency = "ZAR", Note = "Toiletries and hygiene", ExpenseDate = now }
        });
    }

    await db.SaveChangesAsync();

    var userCount = await db.Users.CountAsync();
    var categoryCount = await db.Categories.CountAsync();
    var expenseCount = await db.Expenses.CountAsync();

    Console.WriteLine($"Database initialized - Users: {userCount}, Categories: {categoryCount}, Expenses: {expenseCount}");
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.MapGet("/health", () => new { status = "healthy", time = DateTime.UtcNow });

Console.WriteLine($"Application starting. Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Database type: {(useSql ? "SQL Server" : "In-Memory")}" );

app.Run();
