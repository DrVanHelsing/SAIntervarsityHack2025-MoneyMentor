namespace MoneyMentor.Shared.DTOs;

public record ExpenseEntryDto(Guid ExpenseId, Guid UserId, int CategoryId, decimal Amount, string Currency, string? Note, DateTime ExpenseDate);

public record CategoryDto(int CategoryId, string Name, string? Description);

public record AdviceRequestDto(Guid UserId, string Question, string Language, List<ExpenseEntryDto>? Expenses = null);
public record AdviceResponseDto(string Answer, string Language);

public record ChatMessageDto(Guid ConversationId, string Sender, string Text, DateTime CreatedAt);
public record ConversationDto(Guid ConversationId, Guid UserId, DateTime StartedAt, DateTime? EndedAt);
