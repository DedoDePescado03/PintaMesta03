using Microsoft.Extensions.DependencyInjection;
using PintaMesta.Models;
using Supabase.Interfaces;
using Supabase;
using Supabase.Postgrest.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Plugin.Maui.Audio;

namespace PintaMesta
{
    public partial class App : Application
    {
        private readonly IAudioManager _audioManager;
        private IAudioPlayer? _musicPlayer;

        public static IServiceProvider Services { get; private set; }

        public App(IServiceProvider serviceProvider, IAudioManager audioManager)
        {
            InitializeComponent();

            Services = serviceProvider;
            _audioManager = audioManager;

            InitializeSupabase();
            _ = InitializeMusicAsync(); 

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

        private async Task InitializeMusicAsync()
        {
            try
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("MusiquitaMenu.wav");
                _musicPlayer = _audioManager.CreatePlayer(stream);
                _musicPlayer.Loop = true;
                _musicPlayer.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al iniciar la música: {ex.Message}");
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            _musicPlayer?.Stop();
            _musicPlayer?.Dispose();
        }
    }
}
