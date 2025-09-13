using Microsoft.AspNetCore.Mvc;
using MoneyMentor.ApiOrchestrator.Services;
using MoneyMentor.Shared.DTOs;

namespace MoneyMentor.ApiOrchestrator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatOrchestrationService _chat;
    public ChatController(ChatOrchestrationService chat) => _chat = chat;

    [HttpPost("message")]
    public async Task<ActionResult<AdviceResponseDto>> PostMessage([FromBody] AdviceRequestDto request)
    {
        var result = await _chat.HandleQuestionAsync(request);
        return Ok(result);
    }
}
