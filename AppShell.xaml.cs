using Microsoft.Extensions.DependencyInjection;

namespace PintaMesta
{
    public partial class AppShell : Shell
    {
        public AppShell(IServiceProvider services)
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(DrawingPage), typeof(DrawingPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        }
    }
}
