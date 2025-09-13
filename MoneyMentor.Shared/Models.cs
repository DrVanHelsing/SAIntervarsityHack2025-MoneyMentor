namespace MoneyMentor.Shared.Models;

using System.ComponentModel.DataAnnotations;

public class User
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = "en-US";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<AdviceLog> AdviceLogs { get; set; } = new List<AdviceLog>();
}

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? UserId { get; set; }
}

public class Expense
{
    public Guid ExpenseId { get; set; }
    public Guid UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string? Note { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Category? Category { get; set; }
}

public class Conversation
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class Message
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public string Sender { get; set; } = "user"; // user|assistant
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AdviceLog
{
    [Key]
    public Guid AdviceId { get; set; }
    public Guid UserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string Language { get; set; } = "en-US";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
