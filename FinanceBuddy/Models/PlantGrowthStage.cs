namespace FinanceBuddy.Models;

public enum PlantGrowthStage
{
    Seed = 0,        // Just started financial journey
    Sprout = 1,      // Basic money awareness (100+ points)
    Seedling = 2,    // Building good habits (300+ points)  
    YoungPlant = 3,  // Developing financial discipline (600+ points)
    MaturePlant = 4, // Money-wise mastery (1000+ points)
    BloomingTree = 5 // Financial flourishing (1500+ points)
}

public static class PlantGrowthHelper
{
    public static PlantGrowthStage GetStageFromPoints(int totalPoints)
    {
        return totalPoints switch
        {
            >= 1500 => PlantGrowthStage.BloomingTree,
            >= 1000 => PlantGrowthStage.MaturePlant,
            >= 600 => PlantGrowthStage.YoungPlant,
            >= 300 => PlantGrowthStage.Seedling,
            >= 100 => PlantGrowthStage.Sprout,
            _ => PlantGrowthStage.Seed
        };
    }
    
    public static string GetStageDisplayName(PlantGrowthStage stage)
    {
        return stage switch
        {
            PlantGrowthStage.Seed => "Starting Journey",
            PlantGrowthStage.Sprout => "Money Aware",
            PlantGrowthStage.Seedling => "Building Habits", 
            PlantGrowthStage.YoungPlant => "Financially Disciplined",
            PlantGrowthStage.MaturePlant => "Money-Wise",
            PlantGrowthStage.BloomingTree => "Financially Flourishing",
            _ => "Unknown"
        };
    }
    
    public static string GetStageImageName(PlantGrowthStage stage)
    {
        return stage switch
        {
            PlantGrowthStage.Seed => "plant_seed.png",
            PlantGrowthStage.Sprout => "plant_sprout.png",
            PlantGrowthStage.Seedling => "plant_seedling.png",
            PlantGrowthStage.YoungPlant => "plant_young.png",
            PlantGrowthStage.MaturePlant => "plant_mature.png",
            PlantGrowthStage.BloomingTree => "plant_blooming.png",
            _ => "plant_seed.png"
        };
    }
    
    public static int GetPointsForNextStage(int currentPoints)
    {
        var currentStage = GetStageFromPoints(currentPoints);
        return currentStage switch
        {
            PlantGrowthStage.Seed => 100,
            PlantGrowthStage.Sprout => 300,
            PlantGrowthStage.Seedling => 600,
            PlantGrowthStage.YoungPlant => 1000,
            PlantGrowthStage.MaturePlant => 1500,
            PlantGrowthStage.BloomingTree => 1500, // Already at max
            _ => 100
        };
    }
    
    public static string GetStageMotivation(PlantGrowthStage stage)
    {
        return stage switch
        {
            PlantGrowthStage.Seed => "Every financial journey starts with a single step! ??",
            PlantGrowthStage.Sprout => "You're becoming more aware of your money! ??",
            PlantGrowthStage.Seedling => "Great habits are taking root! ??",
            PlantGrowthStage.YoungPlant => "Your financial discipline is growing strong! ??",
            PlantGrowthStage.MaturePlant => "You've achieved money wisdom! ??",
            PlantGrowthStage.BloomingTree => "You're financially flourishing! Share your wisdom! ??",
            _ => "Keep growing your financial wellness!"
        };
    }
}