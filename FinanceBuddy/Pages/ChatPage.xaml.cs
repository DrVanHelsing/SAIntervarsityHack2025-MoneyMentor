using System.Collections.ObjectModel;
using FinanceBuddy.Services;
using MoneyMentor.Shared.DTOs;
using MoneyMentor.Shared.Models;
using FinanceBuddy.Models;
using Microsoft.Maui.Controls;

namespace FinanceBuddy.Pages;

public partial class ChatPage : ContentPage
{
    private readonly ApiClient _api;
    private readonly ITranslationService _translationService;
    private readonly IGamificationService _gamificationService;

    public ObservableCollection<ChatBubble> Messages { get; } = new();

    // Cache of latest expenses context (lightweight; updated when sending)
    private List<Expense>? _cachedExpenses;
    private DateTime _lastExpensesFetch = DateTime.MinValue;

    // Default seeded user id from backend seeding / migrations
    private static readonly Guid DefaultUserId = new("00000000-0000-0000-0000-000000000001");

    // Language and speech state
    private SupportedLanguage _currentLanguage;
    private bool _autoDetectEnabled;

    // Parameterless ctor for XAML (Shell DataTemplate) support
    public ChatPage() : this(
        ServiceHelper.GetRequiredService<ApiClient>(), 
        ServiceHelper.GetRequiredService<ITranslationService>(),
        ServiceHelper.GetRequiredService<IGamificationService>()) { }

    public ChatPage(ApiClient apiClient, ITranslationService translationService, IGamificationService gamificationService)
    {
        InitializeComponent();
        _api = apiClient;
        _translationService = translationService;
        _gamificationService = gamificationService;
        _currentLanguage = LanguageHelper.GetDefaultLanguage();

        MessagesView.ItemsSource = Messages;
        InitializeLanguagePicker();
        
        // Seed greeting
        Messages.Add(new ChatBubble { Text = "Hi, I'm your Money Mentor. Ask me a finance question to grow your plant! ??", IsUser = false });
        UpdateDetectedLanguageLabel();
    }

    private void InitializeLanguagePicker()
    {
        LanguagePicker.ItemsSource = LanguageHelper.SupportedLanguages;
        LanguagePicker.ItemDisplayBinding = new Binding("DisplayName");
        LanguagePicker.SelectedItem = _currentLanguage;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (_autoDetectEnabled) return; // ignore manual selection while auto-detect on
        if (LanguagePicker.SelectedItem is SupportedLanguage selectedLanguage)
        {
            _currentLanguage = selectedLanguage;
            UpdateDetectedLanguageLabel();
        }
    }

    private void OnAutoDetectToggled(object? sender, ToggledEventArgs e)
    {
        _autoDetectEnabled = e.Value;
        AutoDetectInfoLabel.Text = _autoDetectEnabled ? "Auto-detect ON" : string.Empty;
        LanguagePicker.IsEnabled = !_autoDetectEnabled;
        if (_autoDetectEnabled)
        {
            DetectedLanguageLabel.Text = "Detecting language automatically...";
        }
        else
        {
            UpdateDetectedLanguageLabel();
        }
    }

    private async Task EnsureExpensesAsync()
    {
        // Only refresh if null or older than 2 minutes (simple heuristic)
        if (_cachedExpenses == null || (DateTime.UtcNow - _lastExpensesFetch) > TimeSpan.FromMinutes(2))
        {
            _cachedExpenses = await _api.GetExpensesAsync() ?? new List<Expense>();
            _lastExpensesFetch = DateTime.UtcNow;
        }
    }

    private async void OnSend(object? sender, EventArgs e)
    {
        var originalUserText = InputEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(originalUserText)) return;
        
        InputEntry.Text = string.Empty;
        Messages.Add(new ChatBubble { Text = originalUserText, IsUser = true, Timestamp = DateTime.Now });
        
        try
        {
            await EnsureExpensesAsync();

            // Auto detect logic
            string userTranslateCode = _currentLanguage.TranslateCode; // default
            if (_autoDetectEnabled)
            {
                var (lang, score) = await _translationService.DetectLanguageWithScoreAsync(originalUserText);
                // Map translator code to supported set
                var mapped = LanguageHelper.GetByTranslateCode(lang);
                // Only switch if confidence reasonable or current is default English
                if (score >= 0.5f && mapped.TranslateCode != _currentLanguage.TranslateCode)
                {
                    _currentLanguage = mapped;
                    userTranslateCode = mapped.TranslateCode;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LanguagePicker.SelectedItem = _currentLanguage; // sync UI
                        UpdateDetectedLanguageLabel(mapped.DisplayName, score);
                    });
                }
                else
                {
                    // still update label with attempt
                    MainThread.BeginInvokeOnMainThread(() => UpdateDetectedLanguageLabel(_currentLanguage.DisplayName, score));
                }
            }

            // Translate user input to English if needed for OpenAI processing
            string textForAI = originalUserText;
            if (!string.Equals(userTranslateCode, "en", StringComparison.OrdinalIgnoreCase))
            {
                textForAI = await _translationService.TranslateTextAsync(originalUserText, userTranslateCode, "en");
            }

            var expenseDtos = _cachedExpenses
                ?.Select(e => new ExpenseEntryDto(e.ExpenseId, e.UserId, e.CategoryId, e.Amount, e.Currency, e.Note, e.ExpenseDate))
                .Take(50) // cap to avoid overly large prompt
                .ToList();

            // Use the default seeded user id and specify the response language
            var req = new AdviceRequestDto(DefaultUserId, textForAI, userTranslateCode, expenseDtos);
            var resp = await _api.AskAsync(req);
            
            string responseText = resp?.Answer ?? "(no response)";
            
            // Award points for engaging with financial learning
            await _gamificationService.LogChatInteractionAsync();
            
            // Translate response back to user's language if needed
            if (!string.Equals(userTranslateCode, "en", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(responseText))
            {
                responseText = await _translationService.TranslateTextAsync(responseText, "en", userTranslateCode);
            }
            
            Messages.Add(new ChatBubble { Text = responseText, IsUser = false, Timestamp = DateTime.Now });
            
            // Refresh plant status after learning interaction
            if (PlantStatus != null)
            {
                await PlantStatus.RefreshAsync();
            }
            
            ScrollToBottom();
        }
        catch (Exception ex)
        {
            Messages.Add(new ChatBubble { Text = $"Error: {ex.Message}", IsUser = false, Timestamp = DateTime.Now });
        }
    }

    private void UpdateDetectedLanguageLabel(string? detectedOverride = null, float? score = null)
    {
        if (_autoDetectEnabled)
        {
            if (detectedOverride == null)
                DetectedLanguageLabel.Text = "Detecting language automatically...";
            else
                DetectedLanguageLabel.Text = score.HasValue
                    ? $"Detected: {detectedOverride} ({score:P0})"
                    : $"Detected: {detectedOverride}";
        }
        else
        {
            DetectedLanguageLabel.Text = $"Selected: {_currentLanguage.DisplayName}";
        }
    }

    private void ScrollToBottom()
    {
        if (Messages.Count == 0) return;
        var last = Messages[^1];
        MainThread.BeginInvokeOnMainThread(() => MessagesView.ScrollTo(last, position: ScrollToPosition.End, animate: true));
    }
}

public class ChatBubble
{
    public string Text { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}