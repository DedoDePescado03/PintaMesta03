using Supabase.Interfaces;
using Supabase;
using Supabase.Postgrest.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PintaMesta.Models;

namespace PintaMesta
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsLoggedIn();
        }

        private async void GoToDrawPage(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(DrawingPage));
        }

        private async void GoToLogin(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }

        private string GenerateCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string GenerateUuid()
        {
            return Guid.NewGuid().ToString();
        }


        private async void CreateASession(object sender, EventArgs e)
        {
            var client = SupabaseClientService.SupabaseClient;
            var user = client.Auth.CurrentUser;

            if(user == null)
            {
                await DisplayAlert("Error", "Primero tienes que iniciar sesión", "Ok");
                return;
            }

            string sessionCode = GenerateCode(6);

            var session = new Session
            {
                Id = GenerateUuid(),
                Code = sessionCode,
                HostUserId = user.Id,
                DrawerUserId = user.Id,
                CreatedAt = DateTime.Now,
                CurrentWord = "Popo",
            };

            try
            {
                Debug.WriteLine($"Host: {session.HostUserId}");
                Debug.WriteLine($"Sesión: {session}");
                var response = await client.From<Session>().Insert(session);
                var createdSession = response.Models.First();

                var player = new SessionPlayer
                {
                    Id = GenerateUuid(),
                    SessionId = createdSession.Id,
                    UserId = user.Id,
                    IsDrawer = true,
                    Score = 0,
                    JoinedAt = DateTime.Now,
                };

                Debug.WriteLine(player);
                await client.From<SessionPlayer>().Insert(player);
                await Shell.Current.GoToAsync($"LobbyPage?sessionId={createdSession.Id}&code={createdSession.Code}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Session failed: {ex}");
                await DisplayAlert("Error", ex.Message, "Ok");
            }

        }

        private void IsLoggedIn()
        {
            var client = SupabaseClientService.SupabaseClient;
            var user = client.Auth.CurrentUser;

            if(user != null)
            {
                Login.IsVisible = false;
            }
        }

    }

}
