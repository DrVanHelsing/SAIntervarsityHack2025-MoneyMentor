using System.Text;
using System.Text.Json;

namespace FinanceBuddy.Services;

public interface ITranslationService
{
    Task<string> TranslateTextAsync(string text, string fromLanguage, string toLanguage);
    Task<string> DetectLanguageAsync(string text);
    Task<(string language, float score)> DetectLanguageWithScoreAsync(string text); // new extended method
}

public class TranslationService : ITranslationService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _subscriptionKey;
    private readonly string _endpoint;
    private readonly string _region;

    public TranslationService()
    {
        _httpClient = new HttpClient();
        
        // Configure Azure Translator service settings
        // TODO: Replace with configuration injection or environment variables in production
        _subscriptionKey = GetTranslatorKey();
        _endpoint = "https://api.cognitive.microsofttranslator.com/";
        _region = "southafricanorth";
        
        if (!string.IsNullOrEmpty(_subscriptionKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _region);
        }
    }

    private string GetTranslatorKey()
    {
        // In production, this should come from secure configuration
        // For demo purposes, we'll use a placeholder that won't work
        // This prevents accidental API usage with hardcoded keys
        
        // Check environment variable first (for production deployment)
        var envKey = Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_KEY");
        if (!string.IsNullOrEmpty(envKey))
        {
            return envKey;
        }
        
        // For demo/development - return empty to disable translation
        // Developers should set their own key in environment variables
        return string.Empty;
    }

    public async Task<string> TranslateTextAsync(string text, string fromLanguage, string toLanguage)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            
            // If same language, no translation needed
            if (fromLanguage.Equals(toLanguage, StringComparison.OrdinalIgnoreCase))
                return text;

            // If no API key available, return original text
            if (string.IsNullOrEmpty(_subscriptionKey))
            {
                System.Diagnostics.Debug.WriteLine("Translation service not configured - API key missing");
                return text;
            }

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

            // If no API key available, default to English
            if (string.IsNullOrEmpty(_subscriptionKey))
            {
                System.Diagnostics.Debug.WriteLine("Language detection service not configured - API key missing");
                return ("en", 1.0f);
            }

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

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
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