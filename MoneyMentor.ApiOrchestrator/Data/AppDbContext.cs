using Microsoft.EntityFrameworkCore;
using MoneyMentor.Shared.Models;

namespace MoneyMentor.ApiOrchestrator.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<AdviceLog> AdviceLogs => Set<AdviceLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships and keys
        modelBuilder.Entity<User>().HasKey(u => u.UserId);
        modelBuilder.Entity<Expense>().HasKey(e => e.ExpenseId);
        modelBuilder.Entity<Category>().HasKey(c => c.CategoryId);
        modelBuilder.Entity<Conversation>().HasKey(c => c.ConversationId);
        modelBuilder.Entity<Message>().HasKey(m => m.MessageId);
        modelBuilder.Entity<AdviceLog>().HasKey(al => al.AdviceId);

        // Configure precision for money values
        modelBuilder.Entity<Expense>()
            .Property(e => e.Amount)
            .HasPrecision(18, 2);

        // Configure Expense relationships
        modelBuilder.Entity<Expense>()
            .HasOne(e => e.User)
            .WithMany(u => u.Expenses)
            .HasForeignKey(e => e.UserId);

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId);

        // Configure Conversation relationships
        modelBuilder.Entity<Conversation>()
            .HasOne<User>()
            .WithMany(u => u.Conversations)
            .HasForeignKey(c => c.UserId);

        // Configure Message relationships
        modelBuilder.Entity<Message>()
            .HasOne<Conversation>()
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId);

        // Configure AdviceLog relationships
        modelBuilder.Entity<AdviceLog>()
            .HasOne<User>()
            .WithMany(u => u.AdviceLogs)
            .HasForeignKey(al => al.UserId);

        // Fixed timestamps for design-time seeding
        var seedTimestamp = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        var defaultUserId = new Guid("00000000-0000-0000-0000-000000000001");

        // Seed initial data (static values only; dynamic seeding handled in Program.cs if needed)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = defaultUserId,
                Name = "DefaultUser",
                Email = "default@example.com",
                CreatedAt = seedTimestamp
            }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Transport" },
            new Category { CategoryId = 2, Name = "Food" },
            new Category { CategoryId = 3, Name = "Health" },
            new Category { CategoryId = 4, Name = "Entertainment" },
            new Category { CategoryId = 5, Name = "Utilities" }
        );
    }
}
