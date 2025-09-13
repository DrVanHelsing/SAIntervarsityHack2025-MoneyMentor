using MoneyMentor.Shared.DTOs;

namespace MoneyMentor.ApiOrchestrator.Services;

public class ChatOrchestrationService
{
    private readonly AdviceService _advice;

    public ChatOrchestrationService(AdviceService advice)
    {
        _advice = advice;
    }

    public Task<AdviceResponseDto> HandleQuestionAsync(AdviceRequestDto request)
        => _advice.GetAdviceAsync(request);
}
