namespace PintaMesta
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(DrawingPage), typeof(DrawingPage));
        }
    }
}
