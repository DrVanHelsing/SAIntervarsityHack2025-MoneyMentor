using FinanceBuddy.Pages;
using FinanceBuddy.Services;

namespace FinanceBuddy
{
    public partial class AppShell : Shell
    {
        private readonly IGamificationService _gamificationService;

        public AppShell()
        {
            InitializeComponent();
            _gamificationService = ServiceHelper.GetRequiredService<IGamificationService>();

            // Register routes for pages to enable dependency injection
            Routing.RegisterRoute(nameof(ExpensesPage), typeof(ExpensesPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
            Routing.RegisterRoute(nameof(GamificationTestPage), typeof(GamificationTestPage));
            
            // Check daily streak when app starts
            _ = Task.Run(async () => await _gamificationService.CheckDailyStreakAsync());
        }
    }
}
