namespace PintaMesta;

using PintaMesta.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Timers;

public partial class LobbyPage : ContentPage, IQueryAttributable
{
    private string _sessionId;
    private Timer _updateTimer;
    private ObservableCollection<string> _usernames = new();
    public LobbyPage()
    {
        InitializeComponent();
        PlayersListView.ItemsSource = _usernames;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("sessionId"))
        {
            _sessionId = query["sessionId"] as string;

            var client = SupabaseClientService.SupabaseClient;

            try
            {
                var sessionResponse = await client.From<Session>().Where(s => s.Id == _sessionId).Get();
                var session = sessionResponse.Models.FirstOrDefault();

                var hostUsernameResponse = await client.From<Profile>().Where(s => s.Id == session.HostUserId).Get();
                var hostUsername = hostUsernameResponse.Models.FirstOrDefault();

                if (session != null)
                {
                    SessionCodeLabel.Text = $"Código de sesión: {session.Code}";
                    SessionName.Text = $"Sesión de {hostUsername.Username}";
                    StartPlayerUpdates();
                }
                else
                {
                    await DisplayAlert("Error", "Sesión no encontrada", "Ok");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar la sesión: {ex}");
                await DisplayAlert("Error", "No se pudo cargar la sesión", "Ok");
            }
        }
    }

    private void StartPlayerUpdates()
    {
        _updateTimer = new Timer(500);
        _updateTimer.Elapsed += async (sender, e) => await LoadPlayers();
        _updateTimer.AutoReset = true;
        _updateTimer.Enabled = true;
    }

    private async Task LoadPlayers()
    {
        var client = SupabaseClientService.SupabaseClient;

        try
        {
            var playerResponse = await client.From<SessionPlayer>().Where(sp => sp.SessionId == _sessionId).Get();
            var playerIds = playerResponse.Models.Select(p => p.UserId).ToList();

            var profileResponse = await client.From<Profile>().Filter("id", Supabase.Postgrest.Constants.Operator.In, playerIds).Get();
            var usernames = profileResponse.Models.Select(u => u.Username).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _usernames.Clear();
                foreach (var username in usernames)
                {
                    _usernames.Add(username);
                }
            });
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error al agregar jugadores: {ex}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
    }

}