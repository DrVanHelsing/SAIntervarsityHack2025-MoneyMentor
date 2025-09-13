namespace FinanceBuddy
{
    public partial class App : Application
    {
        private readonly AppShell _shell;

        public App(AppShell shell)
        {
            InitializeComponent();
            _shell = shell; // store reference; do not set MainPage (obsolete)
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_shell);
        }
    }
}