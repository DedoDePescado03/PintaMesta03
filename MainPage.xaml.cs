using Supabase.Interfaces;
using Supabase;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PintaMesta
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            InitializeSupabase();
        }

        private async void MoveToDrawPage(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(DrawingPage));
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
