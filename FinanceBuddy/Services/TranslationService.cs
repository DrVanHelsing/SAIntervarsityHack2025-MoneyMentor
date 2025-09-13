using System.Text;
using System.Text.Json;

namespace FinanceBuddy.Services;

public interface ITranslationService
{
    Task<string> TranslateTextAsync(string text, string fromLanguage, string toLanguage);
    Task<string> DetectLanguageAsync(string text);
    Task<(string language, float score)> DetectLanguageWithScoreAsync(string text); // new extended method
}

public class TranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    

    public TranslationService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _region);
    }

    public async Task<string> TranslateTextAsync(string text, string fromLanguage, string toLanguage)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            
            // If same language, no translation needed
            if (fromLanguage.Equals(toLanguage, StringComparison.OrdinalIgnoreCase))
                return text;

            string route = $"/translate?api-version=3.0&from={fromLanguage}&to={toLanguage}";
            string requestUri = _endpoint + route;

            var requestBody = new object[]
            {
                new { Text = text }
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var translationResults = JsonSerializer.Deserialize<TranslationResult[]>(responseContent);

            return translationResults?[0]?.Translations?[0]?.Text ?? text;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Translation error: {ex.Message}");
            return text; // Return original text if translation fails
        }
    }

    public async Task<string> DetectLanguageAsync(string text)
    {
        var (language, _) = await DetectLanguageWithScoreAsync(text);
        return language;
    }

    public async Task<(string language, float score)> DetectLanguageWithScoreAsync(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text)) return ("en", 0f);

            string route = "/detect?api-version=3.0";
            string requestUri = _endpoint + route;

            var requestBody = new object[]
            {
                new { Text = text }
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var detectionResults = JsonSerializer.Deserialize<DetectionResult[]>(responseContent);
            var first = detectionResults?.FirstOrDefault();

            return (first?.Language ?? "en", first?.Score ?? 0f);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Language detection error: {ex.Message}");
            return ("en", 0f); // Default to English
        }
    }

    public void Dispose() => _httpClient?.Dispose();
}

// DTOs for translation API responses
public class TranslationResult
{
    public Translation[]? Translations { get; set; }
}

public class Translation
{
    public string? Text { get; set; }
    public string? To { get; set; }
}

public class DetectionResult
{
    public string? Language { get; set; }
    public float Score { get; set; }
}