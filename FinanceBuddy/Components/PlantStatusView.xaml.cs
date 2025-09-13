using FinanceBuddy.Models;
using FinanceBuddy.Services;
using System.ComponentModel;
using CommunityToolkit.Maui.Alerts;

namespace FinanceBuddy.Components;

public partial class PlantStatusView : ContentView, INotifyPropertyChanged
{
    private readonly IGamificationService _gamificationService;
    private GamificationProfile? _profile;

    public PlantStatusView()
    {
        InitializeComponent();
        _gamificationService = ServiceHelper.GetRequiredService<IGamificationService>();
        BindingContext = this;
        
        // Load data when the component is loaded
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        try
        {
            _profile = await _gamificationService.GetProfileAsync();
            OnPropertyChanged(nameof(PlantImageSource));
            OnPropertyChanged(nameof(PlantStageText));
            OnPropertyChanged(nameof(PointsText));
            OnPropertyChanged(nameof(WellnessScoreText));
            OnPropertyChanged(nameof(ProgressToNextStage));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(StreakText));
            OnPropertyChanged(nameof(LearningText));
            OnPropertyChanged(nameof(HasRecentlyLeveledUp));
        }
        catch (Exception ex)
        {
            // Handle error gracefully
            System.Diagnostics.Debug.WriteLine($"Error refreshing plant status: {ex.Message}");
        }
    }

    private async void OnPlantTapped(object? sender, EventArgs e)
    {
        if (_profile == null) return;
        
        try
        {
            // Show detailed plant information
            var motivation = _profile.StageMotivation;
            var detailedInfo = $"{motivation}\n\n" +
                              $"?? Expenses logged: {_profile.TotalExpensesLogged}\n" +
                              $"?? Chat interactions: {_profile.ChatInteractions}\n" +
                              $"?? Lessons read: {_profile.FinancialLessonsRead}\n" +
                              $"?? Goals set: {_profile.SavingsGoalsSet}\n" +
                              $"? Goals achieved: {_profile.SavingsGoalsAchieved}";
            
            var mainPage = Application.Current?.Windows?.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlert(
                    $"Your {_profile.CurrentStageDisplayName} Plant", 
                    detailedInfo, 
                    "Keep Growing!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing plant details: {ex.Message}");
        }
    }

    public string PlantImageSource => _profile?.CurrentStageImageName ?? "plant_seed.png";
    
    public string PlantStageText => _profile?.CurrentStageDisplayName ?? "Getting Started";
    
    public string PointsText => $"{_profile?.TotalPoints ?? 0} pts";
    
    public string WellnessScoreText 
    {
        get
        {
            if (_profile == null) return "Wellness: 0%";
            
            // Calculate wellness score based on diverse activities
            var totalActivities = _profile.TotalExpensesLogged + 
                                _profile.ChatInteractions + 
                                _profile.FinancialLessonsRead + 
                                _profile.SavingsGoalsSet + 
                                _profile.WeeklyReviewsCompleted;
            
            var wellnessScore = Math.Min(100, (totalActivities * 2)); // Cap at 100%
            return $"Wellness: {wellnessScore}%";
        }
    }
    
    public double ProgressToNextStage
    {
        get
        {
            if (_profile == null) return 0.0;
            
            var currentStageMin = _profile.CurrentStage switch
            {
                PlantGrowthStage.Seed => 0,
                PlantGrowthStage.Sprout => 100,
                PlantGrowthStage.Seedling => 300,
                PlantGrowthStage.YoungPlant => 600,
                PlantGrowthStage.MaturePlant => 1000,
                PlantGrowthStage.BloomingTree => 1500,
                _ => 0
            };
            
            var nextStageMin = PlantGrowthHelper.GetPointsForNextStage(_profile.TotalPoints);
            
            if (_profile.CurrentStage == PlantGrowthStage.BloomingTree)
            {
                return 1.0; // Fully grown
            }
            
            var range = nextStageMin - currentStageMin;
            var progress = _profile.TotalPoints - currentStageMin;
            
            return range > 0 ? Math.Min(1.0, Math.Max(0.0, (double)progress / range)) : 0.0;
        }
    }
    
    public string ProgressText
    {
        get
        {
            if (_profile == null) return "";
            
            if (_profile.CurrentStage == PlantGrowthStage.BloomingTree)
            {
                return "Financially flourishing! ??";
            }
            
            var pointsToNext = _profile.PointsToNextStage;
            return $"{pointsToNext} to {PlantGrowthHelper.GetStageDisplayName((PlantGrowthStage)((int)_profile.CurrentStage + 1))}";
        }
    }
    
    public string StreakText => $"{_profile?.CurrentStreak ?? 0}";
    
    public string LearningText => $"{_profile?.FinancialLessonsRead ?? 0}";
    
    public bool HasRecentlyLeveledUp => _profile?.HasRecentlyLeveledUp ?? false;

    public new event PropertyChangedEventHandler? PropertyChanged;
    
    protected new virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}