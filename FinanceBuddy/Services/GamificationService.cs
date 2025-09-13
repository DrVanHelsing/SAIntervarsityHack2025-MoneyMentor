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
    
    public async Task<GamificationProfile> GetProfileAsync()
    {
        await Task.CompletedTask; // For async interface consistency
        
        return new GamificationProfile
        {
            TotalPoints = Preferences.Get(TOTAL_POINTS_KEY, 0),
            CurrentStreak = Preferences.Get(CURRENT_STREAK_KEY, 0),
            LastActiveDate = DateTime.TryParse(Preferences.Get(LAST_ACTIVE_DATE_KEY, ""), out var lastActive) ? lastActive : DateTime.MinValue,
            LastExpenseDate = DateTime.TryParse(Preferences.Get(LAST_EXPENSE_DATE_KEY, ""), out var lastExpense) ? lastExpense : DateTime.MinValue,
            TotalExpensesLogged = Preferences.Get(TOTAL_EXPENSES_KEY, 0),
            DaysWithBudgetCompliance = Preferences.Get(BUDGET_COMPLIANCE_DAYS_KEY, 0),
            FinancialLessonsRead = Preferences.Get(FINANCIAL_LESSONS_KEY, 0),
            SavingsGoalsSet = Preferences.Get(SAVINGS_GOALS_SET_KEY, 0),
            SavingsGoalsAchieved = Preferences.Get(SAVINGS_GOALS_ACHIEVED_KEY, 0),
            ChatInteractions = Preferences.Get(CHAT_INTERACTIONS_KEY, 0),
            WeeklyReviewsCompleted = Preferences.Get(WEEKLY_REVIEWS_KEY, 0),
            LastPlantCelebration = DateTime.TryParse(Preferences.Get(LAST_CELEBRATION_KEY, ""), out var lastCelebration) ? lastCelebration : DateTime.MinValue
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
        
        Preferences.Remove(TOTAL_POINTS_KEY);
        Preferences.Remove(CURRENT_STREAK_KEY);
        Preferences.Remove(LAST_ACTIVE_DATE_KEY);
        Preferences.Remove(LAST_EXPENSE_DATE_KEY);
        Preferences.Remove(TOTAL_EXPENSES_KEY);
        Preferences.Remove(BUDGET_COMPLIANCE_DAYS_KEY);
        Preferences.Remove(FINANCIAL_LESSONS_KEY);
        Preferences.Remove(SAVINGS_GOALS_SET_KEY);
        Preferences.Remove(SAVINGS_GOALS_ACHIEVED_KEY);
        Preferences.Remove(CHAT_INTERACTIONS_KEY);
        Preferences.Remove(WEEKLY_REVIEWS_KEY);
        Preferences.Remove(LAST_CELEBRATION_KEY);
    }
    
    private async Task SaveProfileAsync(GamificationProfile profile)
    {
        await Task.CompletedTask;
        
        Preferences.Set(TOTAL_POINTS_KEY, profile.TotalPoints);
        Preferences.Set(CURRENT_STREAK_KEY, profile.CurrentStreak);
        Preferences.Set(LAST_ACTIVE_DATE_KEY, profile.LastActiveDate.ToString("O"));
        Preferences.Set(LAST_EXPENSE_DATE_KEY, profile.LastExpenseDate.ToString("O"));
        Preferences.Set(TOTAL_EXPENSES_KEY, profile.TotalExpensesLogged);
        Preferences.Set(BUDGET_COMPLIANCE_DAYS_KEY, profile.DaysWithBudgetCompliance);
        Preferences.Set(FINANCIAL_LESSONS_KEY, profile.FinancialLessonsRead);
        Preferences.Set(SAVINGS_GOALS_SET_KEY, profile.SavingsGoalsSet);
        Preferences.Set(SAVINGS_GOALS_ACHIEVED_KEY, profile.SavingsGoalsAchieved);
        Preferences.Set(CHAT_INTERACTIONS_KEY, profile.ChatInteractions);
        Preferences.Set(WEEKLY_REVIEWS_KEY, profile.WeeklyReviewsCompleted);
        Preferences.Set(LAST_CELEBRATION_KEY, profile.LastPlantCelebration.ToString("O"));
    }
}