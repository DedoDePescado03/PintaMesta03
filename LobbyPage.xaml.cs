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
    private bool _isHost = false;
    private string _currentUserId;

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

                    var currentUserId = client.Auth.CurrentUser.Id;
                    _currentUserId = currentUserId;
                    _isHost = session.HostUserId == currentUserId;
                    StartGame.IsVisible = _isHost;
                    Shell.Current.Navigating += OnNavigating;
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
            var sessionCheck = await client.From<Session>().Where(s => s.Id == _sessionId).Get();
            if(sessionCheck.Models.Count == 0)
            {
                if (!_isHost)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        Shell.Current.Navigating -= OnNavigating;
                        await Shell.Current.GoToAsync("//MainPage");
                        await DisplayAlert("Sesión terminada", "El host ha cerrado la sesión", "Ok");
                    });
                    return;
                }
            }
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
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al agregar jugadores: {ex}");
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        _updateTimer?.Stop();
        _updateTimer?.Dispose();

        Shell.Current.Navigating -= OnNavigating;
    }

    private async void ConfirmPlayerExit()
    {
        bool shouldLeave = await DisplayAlert(
            "Salir de la sesión",
            "¿Seguro que quieres salir de la sesión?",
            "Sí", "No");

        if (shouldLeave)
        {
            var client = SupabaseClientService.SupabaseClient;

            await client
                .From<SessionPlayer>()
                .Where(sp => sp.SessionId == _sessionId && sp.UserId == _currentUserId)
                .Delete();

            Debug.WriteLine("Jugador eliminado de la sesión");

            Shell.Current.Navigating -= OnNavigating;
            await Shell.Current.GoToAsync("//MainPage");
        }
    }

    private async void ConfirmHostExit()
    {
        bool shouldLeave = await DisplayAlert(
            "Cerrar sesión",
            "¿Seguro que quieres cerrar la sesión?",
            "Sí", "No");

        if (shouldLeave)
        {
            var client = SupabaseClientService.SupabaseClient;

            await client
                .From<SessionPlayer>()
                .Where(sp => sp.SessionId == _sessionId)
                .Delete();

            await client
                .From<Session>()
                .Where(s => s.Id == _sessionId)
                .Delete();

            Debug.WriteLine("Sesión y jugadores eliminados");

            Shell.Current.Navigating -= OnNavigating;
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
    private void OnBackToolbarItemClicked(object sender, EventArgs e)
    {
        if (_isHost)
        {
            ConfirmHostExit();
        }
        else
        {
            ConfirmPlayerExit();
        }
    }
    protected override bool OnBackButtonPressed()
    {
        if (_isHost)
        {
            ConfirmHostExit();
        }
        else
        {
            ConfirmPlayerExit();
        }

        return true; // Previene el comportamiento por defecto
    }

    private void OnNavigating(object sender, ShellNavigatingEventArgs e)
    {
        if (e.Source == ShellNavigationSource.Pop)
        {
            e.Cancel(); // Cancelamos navegación por ahora

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (_isHost)
                {
                    bool shouldLeave = await Application.Current.MainPage.DisplayAlert(
                        "Cerrar sesión",
                        "¿Seguro que quieres cerrar la sesión?",
                        "Sí", "No");

                    if (shouldLeave)
                    {
                        try
                        {
                            var client = SupabaseClientService.SupabaseClient;

                            await client
                                .From<SessionPlayer>()
                                .Where(sp => sp.SessionId == _sessionId)
                                .Delete();

                            await client
                                .From<Session>()
                                .Where(s => s.Id == _sessionId)
                                .Delete();

                            Debug.WriteLine("Sesión y jugadores eliminados");

                            Shell.Current.Navigating -= OnNavigating;
                            await Shell.Current.GoToAsync("//MainPage");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error al eliminar sesión: {ex.Message}");
                        }
                    }
                }
                else
                {
                    bool shouldLeave = await Application.Current.MainPage.DisplayAlert(
                        "Salir de la sesión",
                        "¿Seguro que quieres salir de la sesión?",
                        "Sí", "No");

                    if (shouldLeave)
                    {
                        try
                        {
                            var client = SupabaseClientService.SupabaseClient;

                            await client
                                .From<SessionPlayer>()
                                .Where(sp => sp.SessionId == _sessionId && sp.UserId == _currentUserId)
                                .Delete();

                            Debug.WriteLine("Jugador eliminado de la sesión");

                            Shell.Current.Navigating -= OnNavigating;
                            await Shell.Current.GoToAsync("//MainPage");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error al eliminar jugador: {ex.Message}");
                        }
                    }
                }
            });
        }
    }
}