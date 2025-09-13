namespace FinanceBuddy.Models;

public class GamificationProfile
{
    public int TotalPoints { get; set; }
    public int CurrentStreak { get; set; }
    public DateTime LastActiveDate { get; set; }
    public DateTime LastExpenseDate { get; set; }
    public int TotalExpensesLogged { get; set; }
    public int DaysWithBudgetCompliance { get; set; }
    public int FinancialLessonsRead { get; set; }
    public int SavingsGoalsSet { get; set; }
    public int SavingsGoalsAchieved { get; set; }
    public int ChatInteractions { get; set; }
    public int WeeklyReviewsCompleted { get; set; }
    public DateTime LastPlantCelebration { get; set; }
    
    // Derived properties
    public PlantGrowthStage CurrentStage => PlantGrowthHelper.GetStageFromPoints(TotalPoints);
    public string CurrentStageDisplayName => PlantGrowthHelper.GetStageDisplayName(CurrentStage);
    public string CurrentStageImageName => PlantGrowthHelper.GetStageImageName(CurrentStage);
    public int PointsToNextStage => Math.Max(0, PlantGrowthHelper.GetPointsForNextStage(TotalPoints) - TotalPoints);
    public string StageMotivation => PlantGrowthHelper.GetStageMotivation(CurrentStage);
    public bool HasRecentlyLeveledUp => (DateTime.Now - LastPlantCelebration).TotalHours < 24;
}

public static class MoneyWiseEvents
{
    // Basic Actions
    public const int ExpenseLoggedPoints = 5;
    public const int FirstExpenseOfDayPoints = 10;
    public const int DailyStreakPoints = 15;
    public const int WeeklyStreakBonusPoints = 50;
    
    // Financial Planning
    public const int BudgetSetPoints = 25;
    public const int BudgetMetPoints = 30;
    public const int SavingsGoalSetPoints = 40;
    public const int SavingsGoalAchievedPoints = 100;
    
    // Learning & Growth
    public const int ChatQuestionAskedPoints = 10;
    public const int FinancialLessonReadPoints = 20;
    public const int WeeklyReviewCompletedPoints = 75;
    
    // Milestones
    public const int FirstWeekCompletePoints = 100;
    public const int FirstMonthCompletePoints = 200;
    public const int DebtReductionPoints = 150;
    
    // Sharing Wisdom (for advanced users)
    public const int HelpedOtherUserPoints = 50;
    public const int SharedFinancialTipPoints = 25;
}