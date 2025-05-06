using Supabase.Interfaces;
using Supabase;
using Supabase.Postgrest.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PintaMesta.Models;
using Plugin.Maui.Audio;

namespace PintaMesta
{
    public partial class MainPage : ContentPage
    {
        private static readonly List<string> WordList = new List<string>
        {
            "Silla", "Mesa", "Lámpara", "Reloj", "Cuchara", "Paraguas", "Mochila", "Televisor", "Celular", "Cepillo",
            "Lentes", "Balón", "Maleta", "Piano", "Espejo", "Taza", "Ventana", "Puerta", "Camiseta", "Zapato",

            "Perro", "Gato", "Elefante", "León", "Pez", "Tiburón", "Águila", "Conejo", "Serpiente", "Tortuga",
            "Delfín", "Rana", "Pato", "Cangrejo", "Ratón", "Oveja", "Vaca", "Caballo", "Jirafa", "Cebra",

            "Árbol", "Flor", "Sol", "Luna", "Nube", "Estrella", "Montaña", "Río", "Lago", "Roca",
            "Nieve", "Lluvia", "Arena", "Volcán", "Isla",

            "Refrigerador", "Microondas", "Sofá", "Cama", "Almohada", "Lavadora", "Cortina", "Alfombra", "Estufa", "Telefono fijo",

            "Auto", "Bicicleta", "Moto", "Camión", "Barco", "Avión", "Helicóptero", "Tren", "Cohete", "Tractor",

            "Manzana", "Plátano", "Pizza", "Helado", "Pan", "Huevo", "Sandía", "Hamburguesa", "Zanahoria", "Uvas",

            "Fantasma", "Payaso", "Globo", "Regalo", "Robot", "Sombrero", "Juguete", "Muñeca", "Espada", "Corona",
            "Dragón", "Sirena", "Ovni", "Calavera", "Castillo"
        };

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsLoggedIn();
        }

        private async Task AnimateButton(Button button)
        {
            await button.ScaleTo(1.1, 100, Easing.SpringIn);
            await button.ScaleTo(1.0, 100, Easing.SpringOut);
        }

        private async void GoToDrawPage(object sender, EventArgs e)
        {
            var button = (Button)sender;
            await AnimateButton(button);

            string word = GetRandomWord();
            await Shell.Current.GoToAsync($"{nameof(DrawingPage)}?word={Uri.EscapeDataString(word)}");
        }

        private async void GoToLogin(object sender, EventArgs e)
        {
            var button = (Button)sender;
            await AnimateButton(button);

            await Shell.Current.GoToAsync(nameof(LoginPage));
        }

        private async void CreateASession(object sender, EventArgs e)
        {
            var button = (Button)sender;
            await AnimateButton(button);

            var client = SupabaseClientService.SupabaseClient;
            var user = client.Auth.CurrentUser;

            if (user == null)
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
                CurrentWord = GetRandomWord(),
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

        private string GetRandomWord()
        {
            var random = new Random();
            int index = random.Next(WordList.Count);
            return WordList[index];
        }

        private void IsLoggedIn()
        {
            var client = SupabaseClientService.SupabaseClient;
            var user = client.Auth.CurrentUser;

            if (user != null)
            {
                Login.IsVisible = false;
            }
        }
    }
}
