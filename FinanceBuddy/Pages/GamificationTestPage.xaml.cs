using FinanceBuddy.Models;
using FinanceBuddy.Services;
using CommunityToolkit.Maui.Alerts;

namespace FinanceBuddy.Pages;

public partial class GamificationTestPage : ContentPage
{
    private readonly IGamificationService _gamificationService;

    // Parameterless ctor for XAML
    public GamificationTestPage() : this(ServiceHelper.GetRequiredService<IGamificationService>()) { }

    public GamificationTestPage(IGamificationService gamificationService)
    {
        InitializeComponent();
        _gamificationService = gamificationService;
    }

    private async void OnReadLessonClicked(object? sender, EventArgs e)
    {
        await _gamificationService.LogFinancialLearningAsync();
        await ShowSuccessAndRefresh("?? Financial lesson completed! +20 points");
    }

    private async void OnWeeklyReviewClicked(object? sender, EventArgs e)
    {
        await _gamificationService.CompleteWeeklyReviewAsync();
        await ShowSuccessAndRefresh("?? Weekly review completed! +75 points");
    }

    private async void OnSetGoalClicked(object? sender, EventArgs e)
    {
        await _gamificationService.SetSavingsGoalAsync();
        await ShowSuccessAndRefresh("?? Savings goal set! +40 points");
    }

    private async void OnAchieveGoalClicked(object? sender, EventArgs e)
    {
        await _gamificationService.AchieveSavingsGoalAsync();
        await ShowSuccessAndRefresh("? Savings goal achieved! +100 points - Great job!");
    }

    private async void OnSetBudgetClicked(object? sender, EventArgs e)
    {
        await _gamificationService.SetBudgetAsync();
        await ShowSuccessAndRefresh("?? Budget set! +25 points");
    }

    private async void OnMeetBudgetClicked(object? sender, EventArgs e)
    {
        await _gamificationService.MeetBudgetAsync();
        await ShowSuccessAndRefresh("?? Budget goal met! +30 points");
    }

    private async void OnFirstWeekClicked(object? sender, EventArgs e)
    {
        await _gamificationService.AddPointsAsync(MoneyWiseEvents.FirstWeekCompletePoints, "First week milestone");
        await ShowSuccessAndRefresh("?? First week milestone! +100 points");
    }

    private async void OnStreakBonusClicked(object? sender, EventArgs e)
    {
        await _gamificationService.AddPointsAsync(MoneyWiseEvents.WeeklyStreakBonusPoints, "Weekly streak bonus");
        await ShowSuccessAndRefresh("?? Weekly streak bonus! +50 points");
    }

    private async void OnResetClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Reset Plant", 
            "Are you sure you want to reset all plant progress? This cannot be undone.", 
            "Reset", "Cancel");
            
        if (confirm)
        {
            await _gamificationService.ResetProfileAsync();
            await ShowSuccessAndRefresh("?? Plant progress reset - Start growing again!");
        }
    }

    private async Task ShowSuccessAndRefresh(string message)
    {
        try
        {
            // Check for level up
            var (leveledUp, newStage) = await _gamificationService.CheckForLevelUpAsync();
            if (leveledUp)
            {
                message += $"\n?? Level up! Now {PlantGrowthHelper.GetStageDisplayName(newStage)}!";
            }

            // Update UI
            StatusLabel.Text = message;
            await PlantStatus.RefreshAsync();

            // Show snackbar
            var snackbar = Snackbar.Make(message);
            await snackbar.Show();

            // Clear status after delay
            await Task.Delay(3000);
            StatusLabel.Text = string.Empty;
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error in ShowSuccessAndRefresh: {ex}");
        }
    }
}