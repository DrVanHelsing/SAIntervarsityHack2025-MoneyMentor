namespace FinanceBuddy.Models;

public class SupportedLanguage
{
    public string Code { get; set; } = string.Empty;          // Full locale code, e.g. en-ZA
    public string SpeechCode { get; set; } = string.Empty;     // (kept for future speech integration)
    public string TranslateCode { get; set; } = string.Empty;  // Translator service code, e.g. en, zu
    public string DisplayName { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
}

public static class LanguageHelper
{
    public static readonly List<SupportedLanguage> SupportedLanguages = new()
    {
        new SupportedLanguage { Code = "en-ZA", SpeechCode = "en-ZA", TranslateCode = "en", DisplayName = "English (South Africa)", NativeName = "English" },
        new SupportedLanguage { Code = "zu-ZA", SpeechCode = "zu-ZA", TranslateCode = "zu", DisplayName = "isiZulu", NativeName = "isiZulu" },
        new SupportedLanguage { Code = "xh-ZA", SpeechCode = "xh-ZA", TranslateCode = "xh", DisplayName = "isiXhosa", NativeName = "isiXhosa" },
        new SupportedLanguage { Code = "af-ZA", SpeechCode = "af-ZA", TranslateCode = "af", DisplayName = "Afrikaans", NativeName = "Afrikaans" }
    };

    public static SupportedLanguage GetLanguageByCode(string code) =>
        SupportedLanguages.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
        ?? SupportedLanguages[0];

    public static SupportedLanguage GetByTranslateCode(string translateCode) =>
        SupportedLanguages.FirstOrDefault(l => l.TranslateCode.Equals(translateCode, StringComparison.OrdinalIgnoreCase))
        ?? SupportedLanguages[0];

    public static SupportedLanguage GetDefaultLanguage() => SupportedLanguages[0];
}