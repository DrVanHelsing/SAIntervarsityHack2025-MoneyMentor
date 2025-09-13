using FinanceBuddy.Pages;

namespace FinanceBuddy
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for pages to enable dependency injection
            Routing.RegisterRoute(nameof(ExpensesPage), typeof(ExpensesPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
        }
    }
}
