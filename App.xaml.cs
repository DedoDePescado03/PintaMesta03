using Microsoft.Extensions.DependencyInjection;
using PintaMesta.Models;
using Supabase.Interfaces;
using Supabase;
using Supabase.Postgrest.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PintaMesta
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            InitializeSupabase();

            Services = serviceProvider;

            MainPage = new AppShell(serviceProvider);
        }

        private async void InitializeSupabase()
        {
            Debug.WriteLine("Connecting to Supabase...");

            try
            {
                await SupabaseClientService.InitializeAsync();
                Debug.WriteLine("Connected");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error trying to connect: {ex.Message}");
            }
        }
    }
}
