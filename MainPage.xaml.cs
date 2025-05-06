using Supabase.Interfaces;
using Supabase;
using Supabase.Postgrest.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PintaMesta.Models;
using PintaMesta.Services;

namespace PintaMesta
{
    public partial class MainPage : ContentPage
    {

        private IOrientationService _orientationService;

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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            IsLoggedIn();

            _orientationService = App.Services.GetRequiredService<IOrientationService>();
            _orientationService.ForcePortrait();

            string[] frames = { "dinosaurio.png", "dinosaurio2.png" };
            int currentFrame = 0;

            await dinoImage.TranslateTo(0, 0, 1500, Easing.BounceOut);
            await logoImage.TranslateTo(0, 0, 1500, Easing.BounceOut);

            Device.StartTimer(TimeSpan.FromSeconds(0.5), () =>
            {
                currentFrame = (currentFrame + 1) % frames.Length;
                dinoImage.Source = frames[currentFrame];
                return true;
            });

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

        private async void GoToJoin(object sender, EventArgs e)
        {
            var button = (Button)sender;
            await AnimateButton(button);

            await Shell.Current.GoToAsync(nameof(JoinSessionPage));
        }

        private async void CreateASession(object sender, EventArgs e)
        {
            var button = (Button)sender;
            await AnimateButton(button);

            var client = SupabaseClientService.SupabaseClient;
            var user = client.Auth.CurrentUser;

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

                var responseUser = await client.From<Profile>().Where(s => s.Id == user.Id).Get();
                var profile = responseUser.Models.FirstOrDefault();
                var userName = profile.Username;
                await Shell.Current.GoToAsync($"LobbyPage?sessionId={createdSession.Id}");
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
                CreateSession.IsVisible = true;
                Join.IsVisible = true;
            }
            else
            {
                CreateSession.IsVisible = false;
                Join.IsVisible = false;
            }
        }
    }
}
