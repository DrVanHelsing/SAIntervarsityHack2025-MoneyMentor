using FinanceBuddy.Services;
using MoneyMentor.Shared.DTOs;
using MoneyMentor.Shared.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using FinanceBuddy.Models;

namespace FinanceBuddy.Pages;

public partial class ExpensesPage : ContentPage
{
    private readonly ApiClient _api;
    private readonly IGamificationService _gamificationService;
    private readonly Guid _userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    
    public ObservableCollection<Expense> Expenses { get; } = new();
    private bool _hasLoadedInitialData = false;

    // Parameterless ctor for XAML DataTemplate
    public ExpensesPage() : this(ServiceHelper.GetRequiredService<ApiClient>(), ServiceHelper.GetRequiredService<IGamificationService>()) { }

    public ExpensesPage(ApiClient apiClient, IGamificationService gamificationService)
    {
        InitializeComponent();
        _api = apiClient;
        _gamificationService = gamificationService;
        Loaded += ExpensesPage_Loaded;
        ExpensesList.ItemsSource = Expenses; // Set ItemsSource in constructor
    }

    private async void ExpensesPage_Loaded(object? sender, EventArgs e)
    {
        if (!_hasLoadedInitialData)
        {
            // Fire-and-forget the seeding, don't await it
            _ = Task.Run(SeedSampleDataAsync);
            _hasLoadedInitialData = true;
        }
        await LoadAsync();
    }

    private async Task SeedSampleDataAsync()
    {
        try
        {
            Debug.WriteLine("Checking for existing expenses...");
            MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Checking existing data...");
            
            // Check if we already have expenses to avoid duplicate seeding
            var existingExpenses = await _api.GetExpensesAsync();
            Debug.WriteLine($"Found {existingExpenses?.Count ?? 0} existing expenses");
            
            if (existingExpenses != null && existingExpenses.Count > 0)
            {
                Debug.WriteLine("Existing data found, skipping seed");
                return;
            }

            Debug.WriteLine("Seeding sample data...");
            MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Setting up sample data...");

            var sampleExpenses = new List<ExpenseEntryDto>
            {
                new(Guid.Empty, _userId, 1, 35.50m, "ZAR", "Bus fare to work", DateTime.Now.AddDays(-1)),
                new(Guid.Empty, _userId, 2, 127.99m, "ZAR", "Groceries - Pick n Pay", DateTime.Now.AddDays(-2)),
                new(Guid.Empty, _userId, 3, 89.50m, "ZAR", "Toiletries and hygiene", DateTime.Now.AddDays(-3)),
                new(Guid.Empty, _userId, 4, 22.00m, "ZAR", "Coffee and snack", DateTime.Now.AddDays(-3)),
                new(Guid.Empty, _userId, 1, 45.00m, "ZAR", "Taxi home", DateTime.Now.AddDays(-4)),
                new(Guid.Empty, _userId, 2, 234.75m, "ZAR", "Weekly groceries", DateTime.Now.AddDays(-5)),
                new(Guid.Empty, _userId, 5, 120.00m, "ZAR", "Electricity top-up", DateTime.Now.AddDays(-6)),
                new(Guid.Empty, _userId, 4, 67.50m, "ZAR", "Lunch with colleagues", DateTime.Now.AddDays(-7)),
                new(Guid.Empty, _userId, 3, 45.99m, "ZAR", "Pharmacy - vitamins", DateTime.Now.AddDays(-8)),
                new(Guid.Empty, _userId, 1, 28.50m, "ZAR", "Bus fare", DateTime.Now.AddDays(-9)),
            };

            foreach (var expense in sampleExpenses)
            {
                Debug.WriteLine($"Adding expense: {expense.Note} - R{expense.Amount}");
                var result = await _api.AddExpenseAsync(expense);
                if (result != null)
                {
                    Debug.WriteLine($"Successfully added expense: {result.ExpenseId}");
                }
                else
                {
                    Debug.WriteLine($"Failed to add expense: {expense.Note}");
                }
                await Task.Delay(100); // Small delay to avoid overwhelming the API
            }

            Debug.WriteLine("Sample data seeding completed");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                StatusLabel.Text = "Sample data added!";
                await LoadAsync(); // Refresh the list after seeding
                await Task.Delay(1500);
                StatusLabel.Text = string.Empty;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error seeding data: {ex}");
            MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = $"Error seeding data: {ex.Message}");
        }
    }

    private async Task LoadAsync()
    {
        Debug.WriteLine("Loading expenses...");
        MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "Loading expenses...");
        try
        {
            var items = await _api.GetExpensesAsync();
            Debug.WriteLine($"Loaded {items?.Count ?? 0} expenses from API");
            
            Expenses.Clear();
            if (items != null && items.Count > 0)
            {
                // Sort by date descending (newest first)
                var sortedItems = items.OrderByDescending(i => i.ExpenseDate).ToList();
                foreach (var item in sortedItems)
                {
                    Debug.WriteLine($"Adding expense to collection: {item.Note} - R{item.Amount}");
                    Expenses.Add(item);
                }
                
                var total = items.Sum(i => i.Amount);
                MainThread.BeginInvokeOnMainThread(() => HeaderSummaryLabel.Text = $"{items.Count} expenses • Total: R {total:F2}");
                Debug.WriteLine($"Updated header with {items.Count} expenses, total R{total:F2}");
            }
            else
            {
                Debug.WriteLine("No expenses found");
                MainThread.BeginInvokeOnMainThread(() => HeaderSummaryLabel.Text = "No expenses yet");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading expenses: {ex}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = $"Error loading: {ex.Message}";
                HeaderSummaryLabel.Text = "Error loading expenses";
            });
        }
        finally
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrEmpty(StatusLabel.Text) || StatusLabel.Text.Contains("Loading"))
                    StatusLabel.Text = string.Empty;
            });
        }
    }

    private void OnShowAddPanel(object? sender, EventArgs e)
    {
        AddPanel.IsVisible = true;
        AmountEntry.Focus();
    }

    private void OnHideAddPanel(object? sender, EventArgs e)
    {
        AddPanel.IsVisible = false;
        ClearForm();
        StatusLabel.Text = string.Empty;
    }

    private void ClearForm()
    {
        AmountEntry.Text = string.Empty;
        CategoryEntry.Text = string.Empty;
        NoteEntry.Text = string.Empty;
        DatePicker.Date = DateTime.Today;
    }

    private async void OnAddExpense(object? sender, EventArgs e)
    {
        if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
        {
            StatusLabel.Text = "💰 Enter a valid positive amount";
            return;
        }
        
        if (!int.TryParse(CategoryEntry.Text, out int categoryId) || categoryId < 1 || categoryId > 5)
        {
            StatusLabel.Text = "📂 Category must be between 1-5";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(NoteEntry.Text))
        {
            StatusLabel.Text = "📝 Please add a description";
            return;
        }

        var note = NoteEntry.Text.Trim();
        var date = DatePicker.Date;

        var dto = new ExpenseEntryDto(Guid.Empty, _userId, categoryId, amount, "ZAR", note, date);
        
        StatusLabel.Text = "💾 Saving expense...";
        Debug.WriteLine($"Adding new expense: {note} - R{amount}");
        
        try
        {
            var created = await _api.AddExpenseAsync(dto);
            if (created == null)
            {
                Debug.WriteLine("Failed to create expense - API returned null");
                StatusLabel.Text = "❌ Failed to add expense";
                return;
            }

            Debug.WriteLine($"Successfully created expense: {created.ExpenseId}");
            
            // Award gamification points for logging expense
            await _gamificationService.LogExpenseAsync();
            
            StatusLabel.Text = "✅ Expense added successfully!";
            ClearForm();
            await LoadAsync();
            AddPanel.IsVisible = false;
            
            // Show gamification feedback
            await ShowGamificationFeedback();
            
            // Refresh plant status
            if (PlantStatus != null)
            {
                await PlantStatus.RefreshAsync();
            }
            
            // Clear success message after a delay
            await Task.Delay(2000);
            if (StatusLabel.Text.Contains("successfully"))
                StatusLabel.Text = string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding expense: {ex}");
            StatusLabel.Text = $"❌ Error: {ex.Message}";
        }
    }
    
    private async Task ShowGamificationFeedback()
    {
        try
        {
            var profile = await _gamificationService.GetProfileAsync();
            var message = $"🌱 Plant grows! +{MoneyWiseEvents.ExpenseLoggedPoints} points";
            
            // Check if it's first expense of the day for bonus
            if (profile.LastExpenseDate.Date != DateTime.Today)
            {
                message += $" (+{MoneyWiseEvents.FirstExpenseOfDayPoints} daily bonus)";
            }
            
            // Check for level up
            var (leveledUp, newStage) = await _gamificationService.CheckForLevelUpAsync();
            if (leveledUp)
            {
                message += $"\n🥳 Level up! Now {PlantGrowthHelper.GetStageDisplayName(newStage)}!";
            }
            
            var snackbar = Snackbar.Make(message);
            await snackbar.Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing gamification feedback: {ex}");
        }
    }
}
