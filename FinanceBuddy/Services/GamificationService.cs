using FinanceBuddy.Models;

namespace FinanceBuddy.Services;

public interface IGamificationService
{
    Task<GamificationProfile> GetProfileAsync();
    Task<bool> LogExpenseAsync();
    Task<bool> CheckDailyStreakAsync();
    Task<int> AddPointsAsync(int points, string reason);
    Task<bool> LogFinancialLearningAsync();
    Task<bool> LogChatInteractionAsync();
    Task<bool> SetSavingsGoalAsync();
    Task<bool> AchieveSavingsGoalAsync();
    Task<bool> CompleteWeeklyReviewAsync();
    Task<bool> SetBudgetAsync();
    Task<bool> MeetBudgetAsync();
    Task<(bool leveledUp, PlantGrowthStage newStage)> CheckForLevelUpAsync();
    Task ResetProfileAsync();
}

public class GamificationService : IGamificationService
{
    private const string TOTAL_POINTS_KEY = "gamification_total_points";
    private const string CURRENT_STREAK_KEY = "gamification_current_streak";
    private const string LAST_ACTIVE_DATE_KEY = "gamification_last_active_date";
    private const string LAST_EXPENSE_DATE_KEY = "gamification_last_expense_date";
    private const string TOTAL_EXPENSES_KEY = "gamification_total_expenses";
    private const string BUDGET_COMPLIANCE_DAYS_KEY = "gamification_budget_compliance_days";
    private const string FINANCIAL_LESSONS_KEY = "gamification_financial_lessons";
    private const string SAVINGS_GOALS_SET_KEY = "gamification_savings_goals_set";
    private const string SAVINGS_GOALS_ACHIEVED_KEY = "gamification_savings_goals_achieved";
    private const string CHAT_INTERACTIONS_KEY = "gamification_chat_interactions";
    private const string WEEKLY_REVIEWS_KEY = "gamification_weekly_reviews";
    private const string LAST_CELEBRATION_KEY = "gamification_last_celebration";
    
    // Cross-platform safe preference access with proper type handling
    private static int GetIntPreference(string key, int defaultValue)
    {
        try
        {
            return Microsoft.Maui.Storage.Preferences.Get(key, defaultValue);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting int preference {key}: {ex.Message}");
            return defaultValue;
        }
    }
    
    private static string GetStringPreference(string key, string defaultValue)
    {
        try
        {
            return Microsoft.Maui.Storage.Preferences.Get(key, defaultValue);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting string preference {key}: {ex.Message}");
            return defaultValue;
        }
    }
    
    private static void SetIntPreference(string key, int value)
    {
        try
        {
            Microsoft.Maui.Storage.Preferences.Set(key, value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting int preference {key}: {ex.Message}");
        }
    }
    
    private static void SetStringPreference(string key, string value)
    {
        try
        {
            Microsoft.Maui.Storage.Preferences.Set(key, value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting string preference {key}: {ex.Message}");
        }
    }
    
    private static void RemovePreference(string key)
    {
        try
        {
            Microsoft.Maui.Storage.Preferences.Remove(key);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing preference {key}: {ex.Message}");
        }
    }
    
    public async Task<GamificationProfile> GetProfileAsync()
    {
        await Task.CompletedTask; // For async interface consistency
        
        return new GamificationProfile
        {
            TotalPoints = GetIntPreference(TOTAL_POINTS_KEY, 0),
            CurrentStreak = GetIntPreference(CURRENT_STREAK_KEY, 0),
            LastActiveDate = DateTime.TryParse(GetStringPreference(LAST_ACTIVE_DATE_KEY, ""), out var lastActive) ? lastActive : DateTime.MinValue,
            LastExpenseDate = DateTime.TryParse(GetStringPreference(LAST_EXPENSE_DATE_KEY, ""), out var lastExpense) ? lastExpense : DateTime.MinValue,
            TotalExpensesLogged = GetIntPreference(TOTAL_EXPENSES_KEY, 0),
            DaysWithBudgetCompliance = GetIntPreference(BUDGET_COMPLIANCE_DAYS_KEY, 0),
            FinancialLessonsRead = GetIntPreference(FINANCIAL_LESSONS_KEY, 0),
            SavingsGoalsSet = GetIntPreference(SAVINGS_GOALS_SET_KEY, 0),
            SavingsGoalsAchieved = GetIntPreference(SAVINGS_GOALS_ACHIEVED_KEY, 0),
            ChatInteractions = GetIntPreference(CHAT_INTERACTIONS_KEY, 0),
            WeeklyReviewsCompleted = GetIntPreference(WEEKLY_REVIEWS_KEY, 0),
            LastPlantCelebration = DateTime.TryParse(GetStringPreference(LAST_CELEBRATION_KEY, ""), out var lastCelebration) ? lastCelebration : DateTime.MinValue
        };
    }
    
    public async Task<bool> LogExpenseAsync()
    {
        var profile = await GetProfileAsync();
        var today = DateTime.Today;
        
        // Award points for logging expense
        var pointsAwarded = MoneyWiseEvents.ExpenseLoggedPoints;
        
        // Bonus points if this is the first expense of the day
        if (profile.LastExpenseDate.Date != today)
        {
            pointsAwarded += MoneyWiseEvents.FirstExpenseOfDayPoints;
        }
        
        // Update profile
        profile.TotalPoints += pointsAwarded;
        profile.TotalExpensesLogged++;
        profile.LastExpenseDate = DateTime.Now;
        
        await SaveProfileAsync(profile);
        
        // Check for streak update
        await CheckDailyStreakAsync();
        
        return true;
    }
    
    public async Task<bool> LogFinancialLearningAsync()
    {
        var profile = await GetProfileAsync();
        profile.TotalPoints += MoneyWiseEvents.FinancialLessonReadPoints;
        profile.FinancialLessonsRead++;
        await SaveProfileAsync(profile);
        return true;
    }
    
    public async Task<bool> LogChatInteractionAsync()
    {
        var profile = await GetProfileAsync();
        profile.TotalPoints += MoneyWiseEvents.ChatQuestionAskedPoints;
        profile.ChatInteractions++;
        await SaveProfileAsync(profile);
        return true;
    }
    
    public async Task<bool> SetSavingsGoalAsync()
    {
        var profile = await GetProfileAsync();
        profile.TotalPoints += MoneyWiseEvents.SavingsGoalSetPoints;
        profile.SavingsGoalsSet++;
        await SaveProfileAsync(profile);
        return true;
    }
    
    public async Task<bool> AchieveSavingsGoalAsync()
    {
        var profile = await GetProfileAsync();
        profile.TotalPoints += MoneyWiseEvents.SavingsGoalAchievedPoints;
        profile.SavingsGoalsAchieved++;
        await SaveProfileAsync(profile);
        return true;
    }
    
    public async Task<bool> CompleteWeeklyReviewAsync()
    {
        var profile = await GetProfileAsync();
        profile.TotalPoints += MoneyWiseEvents.WeeklyReviewCompletedPoints;
        profile.WeeklyReviewsCompleted++;
        await SaveProfileAsync(profile);
        return true;
    }
    
    public async Task<bool> SetBudgetAsync()
    {
        var profile = await GetProfileAsync();
        profile.TotalPoints += MoneyWiseEvents.BudgetSetPoints;
        await SaveProfileAsync(profile);
        return true;
    }
    
    public async Task<bool> MeetBudgetAsync()
    {
        var profile = await GetProfileAsync();
        profile.TotalPoints += MoneyWiseEvents.BudgetMetPoints;
        profile.DaysWithBudgetCompliance++;
        await SaveProfileAsync(profile);
        return true;
    }
    
    public async Task<(bool leveledUp, PlantGrowthStage newStage)> CheckForLevelUpAsync()
    {
        var profile = await GetProfileAsync();
        var currentStage = profile.CurrentStage;
        
        // Simulate points check (in real implementation, this would track previous stage)
        var previousPoints = profile.TotalPoints - 50; // Approximate previous points
        var previousStage = PlantGrowthHelper.GetStageFromPoints(Math.Max(0, previousPoints));
        
        if (currentStage > previousStage)
        {
            profile.LastPlantCelebration = DateTime.Now;
            await SaveProfileAsync(profile);
            return (true, currentStage);
        }
        
        return (false, currentStage);
    }
    
    public async Task<bool> CheckDailyStreakAsync()
    {
        var profile = await GetProfileAsync();
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        
        bool streakUpdated = false;
        
        if (profile.LastActiveDate.Date == yesterday)
        {
            // Continue streak
            profile.CurrentStreak++;
            profile.TotalPoints += MoneyWiseEvents.DailyStreakPoints;
            
            // Weekly bonus
            if (profile.CurrentStreak % 7 == 0)
            {
                profile.TotalPoints += MoneyWiseEvents.WeeklyStreakBonusPoints;
            }
            
            streakUpdated = true;
        }
        else if (profile.LastActiveDate.Date == today)
        {
            // Already active today, no change needed
        }
        else
        {
            // Streak broken, reset to 1 (for today)
            profile.CurrentStreak = 1;
            streakUpdated = true;
        }
        
        profile.LastActiveDate = DateTime.Now;
        await SaveProfileAsync(profile);
        
        return streakUpdated;
    }
    
    public async Task<int> AddPointsAsync(int points, string reason)
    {
        var profile = await GetProfileAsync();
        profile.TotalPoints += points;
        await SaveProfileAsync(profile);
        return profile.TotalPoints;
    }
    
    public async Task ResetProfileAsync()
    {
        await Task.CompletedTask;
        
        RemovePreference(TOTAL_POINTS_KEY);
        RemovePreference(CURRENT_STREAK_KEY);
        RemovePreference(LAST_ACTIVE_DATE_KEY);
        RemovePreference(LAST_EXPENSE_DATE_KEY);
        RemovePreference(TOTAL_EXPENSES_KEY);
        RemovePreference(BUDGET_COMPLIANCE_DAYS_KEY);
        RemovePreference(FINANCIAL_LESSONS_KEY);
        RemovePreference(SAVINGS_GOALS_SET_KEY);
        RemovePreference(SAVINGS_GOALS_ACHIEVED_KEY);
        RemovePreference(CHAT_INTERACTIONS_KEY);
        RemovePreference(WEEKLY_REVIEWS_KEY);
        RemovePreference(LAST_CELEBRATION_KEY);
    }
    
    private async Task SaveProfileAsync(GamificationProfile profile)
    {
        await Task.CompletedTask;
        
        SetIntPreference(TOTAL_POINTS_KEY, profile.TotalPoints);
        SetIntPreference(CURRENT_STREAK_KEY, profile.CurrentStreak);
        SetStringPreference(LAST_ACTIVE_DATE_KEY, profile.LastActiveDate.ToString("O"));
        SetStringPreference(LAST_EXPENSE_DATE_KEY, profile.LastExpenseDate.ToString("O"));
        SetIntPreference(TOTAL_EXPENSES_KEY, profile.TotalExpensesLogged);
        SetIntPreference(BUDGET_COMPLIANCE_DAYS_KEY, profile.DaysWithBudgetCompliance);
        SetIntPreference(FINANCIAL_LESSONS_KEY, profile.FinancialLessonsRead);
        SetIntPreference(SAVINGS_GOALS_SET_KEY, profile.SavingsGoalsSet);
        SetIntPreference(SAVINGS_GOALS_ACHIEVED_KEY, profile.SavingsGoalsAchieved);
        SetIntPreference(CHAT_INTERACTIONS_KEY, profile.ChatInteractions);
        SetIntPreference(WEEKLY_REVIEWS_KEY, profile.WeeklyReviewsCompleted);
        SetStringPreference(LAST_CELEBRATION_KEY, profile.LastPlantCelebration.ToString("O"));
    }
}