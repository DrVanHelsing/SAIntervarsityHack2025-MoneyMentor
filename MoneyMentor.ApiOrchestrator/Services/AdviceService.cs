using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Azure; // for RequestFailedException
using MoneyMentor.Shared.DTOs;
using System.Text;

namespace MoneyMentor.ApiOrchestrator.Services;

public class AdviceService
{
    private readonly ChatClient? _chatClient;
    private readonly string _deploymentName;
    private readonly string _systemPrompt;
    private bool _enabled;
    private readonly ILogger<AdviceService> _logger;

    public AdviceService(IConfiguration config, ILogger<AdviceService> logger)
    {
        _logger = logger;
        var endpoint = config["AzureOpenAI:Endpoint"];
        var key = config["AzureOpenAI:ApiKey"];
        // Deployment name == model name per user instruction
        _deploymentName = config["AzureOpenAI:DeploymentName"] ?? "gpt-4o-mini";

        _enabled = !string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(key);
        if (_enabled)
        {
            try
            {
                // gpt-4o-mini supported on 2024-05-01-preview
                var clientOptions = new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2024_05_01_Preview);
                var azureClient = new AzureOpenAIClient(new Uri(endpoint!), new ApiKeyCredential(key!), clientOptions);
                _chatClient = azureClient.GetChatClient(_deploymentName);
                _logger.LogInformation("Azure OpenAI chat client initialized. Deployment/Model: {Deployment}", _deploymentName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure OpenAI chat client");
                _enabled = false;
            }
        }
        else
        {
            _logger.LogWarning("Azure OpenAI not configured. Missing endpoint or API key.");
        }

        _systemPrompt = """
You are MoneyMentor, an expert South African personal finance assistant. Your goals:
1. Educational, non?personalised guidance with concise disclaimers when advice-like.
2. South African context: ZAR (R), SARS, TFSA limits, RA, PAYE, UIF, Medical Aid, National Credit Act, FSCA, JSE, inflation, respectful handling of Black Tax.
3. Structure substantial answers:
   A. Quick Summary
   B. Key Considerations
   C. Suggested Steps / Framework
   D. Example / Illustration (if useful)
   E. Risks & Caveats
   F. Educational Disclaimer
4. Ask clarifying questions first if user intent / data insufficient.
5. Budget: adapt frameworks (50/30/20 etc.) to user signals; explain rationale briefly.
6. Debt: classify types; prioritise high?interest; emergency fund (3�6 months) before aggressive investing.
7. Investing: no specific securities; use categories (broad-market ETF, balanced unit trust, bond fund, money market, offshore equity ETF); highlight diversification, risk, fees (TER), tax wrappers (TFSA, RA), CGT basics.
8. Retirement: compounding, inflation?adjusted real return mindset, contribution escalation.
9. Tax: always note you are not a tax practitioner; reference SARS for authoritative guidance.
10. Offshore / FX: exchange control allowances, rand volatility, diversification rationale.
11. Language: respond in the language requested by the user. If English, use South African English expressions and context.
12. Refuse disallowed or unrelated requests politely; redirect to finance topics.
13. Tone: always speak naturally, encouraging, neutral, never judgmental.
14. Currency: format as R1 234.56 (space separator) unless small (R120). Show simple workings when calculating.
When expense context is supplied, reference patterns/trends (categories, totals, simple monthly extrapolation) but avoid over?personalised advice.
15. Do not use special characters (�, ?, etc.) in your responses. the tone must be converational and friendly.
""";
    }

    public async Task<AdviceResponseDto> GetAdviceAsync(AdviceRequestDto request)
    {
        if (!_enabled || _chatClient is null)
        {
            var fallback = GetLanguageSpecificFallback(request.Language);
            _logger.LogWarning("Azure OpenAI service called but not enabled/configured");
            return new AdviceResponseDto(fallback, request.Language);
        }

        try
        {
            var contextBuilder = new StringBuilder();
            if (request.Expenses is { Count: > 0 })
            {
                var total = request.Expenses.Sum(e => e.Amount);
                var byCategory = request.Expenses.GroupBy(e => e.CategoryId)
                    .Select(g => new { CategoryId = g.Key, Total = g.Sum(x => x.Amount), Count = g.Count() })
                    .OrderByDescending(g => g.Total);
                contextBuilder.AppendLine("Expense Context:");
                contextBuilder.AppendLine($"Items: {request.Expenses.Count}  Total: R {total:F2}");
                foreach (var cat in byCategory)
                {
                    contextBuilder.AppendLine($" - Category {cat.CategoryId}: R {cat.Total:F2} ({cat.Count} items)");
                }
            }

            var languageInstruction = GetLanguageInstruction(request.Language);
            var userPrompt = contextBuilder.Length > 0
                ? contextBuilder + "\nUser Question: " + request.Question
                : request.Question;

            List<ChatMessage> messages =
            [
                new SystemChatMessage(_systemPrompt),
                new UserChatMessage($"UserId: {request.UserId}\n{languageInstruction}\n{userPrompt}")
            ];

            var options = new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxTokens = 700,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            };

            _logger.LogInformation("Sending chat completion request. Deployment/Model: {Deployment} (Language: {Language}, Expense ctx: {HasCtx})", 
                _deploymentName, request.Language, request.Expenses is { Count: > 0 });
            
            var result = await _chatClient.CompleteChatAsync(messages, options);
            var answer = result.Value.Content.FirstOrDefault()?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(answer))
            {
                answer = GetLanguageSpecificFallback(request.Language);
                _logger.LogWarning("Azure OpenAI returned empty response");
            }
            else
            {
                _logger.LogInformation("Received response from Azure OpenAI");
            }

            return new AdviceResponseDto(answer!, request.Language);
        }
        catch (RequestFailedException rfe)
        {
            _logger.LogError(rfe, "Azure OpenAI request failed (Status {Status})", rfe.Status);
            var fallback = GetLanguageSpecificFallback(request.Language);
            return new AdviceResponseDto(fallback, request.Language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Azure OpenAI service");
            var fallback = GetLanguageSpecificFallback(request.Language);
            return new AdviceResponseDto(fallback, request.Language);
        }
    }

    private string GetLanguageInstruction(string languageCode)
    {
        return languageCode.ToLower() switch
        {
            "en" or "en-za" => "Please respond in South African English.",
            "zu" or "zu-za" => "Please respond in isiZulu (with English financial terms where appropriate).",
            "xh" or "xh-za" => "Please respond in isiXhosa (with English financial terms where appropriate).",
            "af" or "af-za" => "Please respond in Afrikaans (with English financial terms where appropriate).",
            _ => "Please respond in English."
        };
    }

    private string GetLanguageSpecificFallback(string languageCode)
    {
        return languageCode.ToLower() switch
        {
            "zu" or "zu-za" => "Azure OpenAI ayisebenzi manje. Imigomo ebalulekile: yakha imali yokucima umlilo, unciphise izikweletu ezinesintelo esiphakeme, sebenzisa i-TFSA ne-RA, hlanganisa izindlela zokutshala imali.",
            "xh" or "xh-za" => "I-Azure OpenAI ayisebenzi ngoku. Imigaqo ebalulekileyo: yakha imali yongxibelelwano, unciphise amatyala aneenzala eziphezulu, sebenzisa i-TFSA ne-RA, yandisa iindlela zotyalo-mali.",
            "af" or "af-za" => "Azure OpenAI is nie beskikbaar nie. Kernbeginsels: bou noodfonds, verminder hoë-rente skuld, gebruik TFSA/RA, diversifiseer beleggings.",
            _ => "Azure OpenAI not configured. Core principles: Build emergency fund, reduce high-interest debt, contribute to TFSA/RA, diversify investments."
        };
    }
}
