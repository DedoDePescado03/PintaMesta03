namespace PintaMesta;
using PintaMesta.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using CommunityToolkit.Maui.Core.Views;
using PintaMesta.Models;
using System.Text.Json;
using System.Collections.ObjectModel;
using static Supabase.Postgrest.Constants;
using System.Diagnostics;
using Supabase.Realtime;
using Supabase.Realtime.PostgresChanges;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Supabase.Interfaces;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls.Compatibility;

public partial class GuessingPage : ContentPage, IQueryAttributable
{
    private IOrientationService _orientationService;
    private System.Timers.Timer _timer;
    private string _sessionId;
    private RealtimeChannel _channel;
    private RealtimeChannel _channelGuess;
    private RealtimeChannel _roundChannel;
    private DrawingView _drawingView;

    public ObservableCollection<string> Guesses {  get; set; } = new();

    public GuessingPage()
	{
		InitializeComponent();
        BindingContext = this;
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("sessionId"))
            _sessionId = query["sessionId"] as string;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        _drawingView = new DrawingView
        {
            InputTransparent = true,
            BackgroundColor = Colors.LightGray,
            IsMultiLineModeEnabled = true,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand,
            Lines = new ObservableCollection<IDrawingLine>()
        };

        DrawingContainer.Content = _drawingView;

        // Limpia estado visual
        Guess.IsEnabled = true;
        _drawingView.Lines.Clear();
        Guesses.Clear(); // 



        _orientationService = App.Services.GetRequiredService<IOrientationService>();
        _orientationService.ForceLandscape();

        // Suscripciones solo si no existen ya
        await SubscribeToDrawingUpdates();
        await SubscribeToNewGuesses();
        await SubscribeToRoundUpdates();

        // Forzar recarga del dibujo al volver a GuessingPage
        await FetchDrawings();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _orientationService?.AllowOrientations();

        DrawingContainer.Content = null;
        _drawingView = null;

        _channel?.Unsubscribe();
        _channel = null;

        _channelGuess?.Unsubscribe();
        _channelGuess = null;

        _roundChannel?.Unsubscribe();
        _roundChannel = null;

        Console.WriteLine("Desuscrito de todo");
    }

    private async Task SubscribeToDrawingUpdates()
    {
        _channel = SupabaseClientService.SupabaseClient.Realtime
        .Channel("realtime","public","drawing_data");

        _channel.AddPostgresChangeHandler(PostgresChangesOptions.ListenType.All, async (sender, change) =>
        {
            var jsonProperty = change.GetType().GetProperty("Json", BindingFlags.NonPublic | BindingFlags.Instance);
            string jsonString = jsonProperty?.GetValue(change)?.ToString();

            var json = JObject.Parse(jsonString);
            jsonString = json.ToString(Newtonsoft.Json.Formatting.Indented);

            var payloadData = json["payload"]?["data"];
            if (payloadData != null)
            {
                var updatedData = payloadData["record"];
                Console.WriteLine("Updated Drawing:");
                Console.WriteLine($"ID: {updatedData["id"]}");
                Console.WriteLine($"Sid: {updatedData["session_id"]}");
                Console.WriteLine($"Points: {updatedData["points"]}");
                Console.WriteLine($"CreatedAt: {updatedData["created_at"]}");
                Console.WriteLine($"Cleared: {updatedData["is_cleared"]}");

                if (updatedData["session_id"]?.ToString() != _sessionId)
                {
                    Console.WriteLine("Evento ignorado: sesión no coincide.");
                    return;
                }

                Console.WriteLine("Evento válido para esta sesión. Actualizando dibujo...");
                await MainThread.InvokeOnMainThreadAsync(() => _ = FetchDrawings());
            }
        });

        await _channel.Subscribe();
        Debug.WriteLine("Suscrito");
    }


    private async Task FetchDrawings()
    {
        try
        {
            var client = SupabaseClientService.SupabaseClient;

            // Obtener current_drawing_id desde la sesión
            var sessionResponse = await client
                .From<Session>()
                .Where(s => s.Id == _sessionId)
                .Get();

            var session = sessionResponse.Models.FirstOrDefault();
            var currentDrawingId = session?.CurrentDrawingId;

            if (string.IsNullOrEmpty(currentDrawingId))
            {
                Console.WriteLine("No hay drawing actual para esta sesión.");
                MainThread.BeginInvokeOnMainThread(() => _drawingView.Lines.Clear());
                return;
            }

            // Obtener solo ese drawing
            var drawingResponse = await client
                .From<DrawingData>()
                .Where(d => d.Id == currentDrawingId)
                .Get();

            var drawing = drawingResponse.Models.FirstOrDefault();

            if (drawing == null)
            {
                Console.WriteLine("No se encontró el dibujo actual.");
                MainThread.BeginInvokeOnMainThread(() => _drawingView.Lines.Clear());
                return;
            }

            // Siempre limpiar antes de cargar el dibujo actual (no acumular)
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _drawingView.Lines.Clear();
            });

            if (drawing.IsCleared == true)
            {
                // Si el dibujo está marcado como borrado, solo limpiar
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _drawingView.Lines.Clear();
                });
                return;
            }

            // Deserializar y cargar líneas del dibujo actual
            var lines = JsonSerializer.Deserialize<List<SerializedLine>>(drawing.Points);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var lineData in lines)
                {
                    var line = new DrawingLine
                    {
                        Points = new ObservableCollection<PointF>(lineData.Points.Select(p => new PointF(p.X, p.Y))),
                        LineColor = Color.FromHex(lineData.Color),
                        LineWidth = lineData.Width,
                    };
                    _drawingView.Lines.Add(line);
                    Console.WriteLine("Se actualizó el Drawing.Lines");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }



    private async Task SendGuessToSupabase(string text, string username)
    {

        try
        {
            var client = SupabaseClientService.SupabaseClient;
            var guess = new Guess
            {
                SessionId = _sessionId,
                Username = username,
                GuessText = text
            };

            await client.From<Guess>().Insert(guess);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al subir el guess: {ex}");
        }

    }

    private async void SendGuess(object sender, EventArgs e)
    {
        var client = SupabaseClientService.SupabaseClient;
        var currentUserId = client.Auth.CurrentUser.Id;

        var sessionResponse = await client
                                .From<Session>()
                                .Where(s => s.Id == _sessionId)
                                .Get();
        var session = sessionResponse.Models.FirstOrDefault();
        string currentWord = session.CurrentWord;
        if (Guess.Text.Trim().ToUpperInvariant() == currentWord.Trim().ToUpperInvariant())
        {
            var playerResponse = await client
                                        .From<SessionPlayer>()
                                        .Where(s => s.UserId == currentUserId)
                                        .Get();
            var playerScored = playerResponse.Models.FirstOrDefault();
            var score = playerScored.Score;
            await client
                .From<SessionPlayer>()
                .Where (s => s.UserId == currentUserId)
                .Set(s => s.Score, score + 1)
                .Update();

            Console.WriteLine("Score aumentado");
        }

        var currentProfile = await client.From<Profile>().Where(u => u.Id == currentUserId).Get();
        var profile = currentProfile.Models.FirstOrDefault();


        if(!string.IsNullOrEmpty(Guess.Text) && profile != null)
        {
            SendGuessToSupabase(Guess.Text.Trim(), profile.Username);
            Guess.Text = string.Empty;
        }
    }

    public async Task SubscribeToNewGuesses()
    {
        _channelGuess = SupabaseClientService.SupabaseClient.Realtime
            .Channel("realtime", "public", "guesses");

        _channelGuess.AddPostgresChangeHandler(PostgresChangesOptions.ListenType.All, async (sender, change) =>
        {
            var jsonProperty = change.GetType().GetProperty("Json", BindingFlags.NonPublic | BindingFlags.Instance);
            string jsonString = jsonProperty?.GetValue(change)?.ToString();

            var json = JObject.Parse(jsonString);
            jsonString = json.ToString(Newtonsoft.Json.Formatting.Indented);

            var payloadData = json["payload"]?["data"];
            if (payloadData != null)
            {
                var updatedData = payloadData["record"];
                Console.WriteLine("Nueva Adivinanza:");
                Console.WriteLine($"ID: {updatedData["id"]}");
                Console.WriteLine($"SessionID: {updatedData["session_id"]}");
                Console.WriteLine($"Username: {updatedData["username"]}");
                Console.WriteLine($"Guess: {updatedData["guess"]}");

                if (updatedData["session_id"]?.ToString() != _sessionId)
                {
                    Console.WriteLine("Adivinanza ignorada: sesión no coincide.");
                    return;
                }

                Console.WriteLine("Adivinanza válida. Mostrando en pantalla...");
                var client = SupabaseClientService.SupabaseClient;
                var sessionResponse = await client
                                        .From<Session>()
                                        .Where(s => s.Id == _sessionId )
                                        .Get();
                var session = sessionResponse.Models.FirstOrDefault();
                string currentWord = session.CurrentWord;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var incomingGuess = updatedData["guess"]?.ToString()?.Trim().ToUpperInvariant() ?? string.Empty;
                    var correctWord = currentWord?.Trim().ToUpperInvariant() ?? string.Empty;

                    if (incomingGuess == correctWord)
                    {
                        string guessingUsername = updatedData["username"]?.ToString();
                        string currentUserId = SupabaseClientService.SupabaseClient.Auth.CurrentUser.Id;
                        var currentUsernameResponse = await client
                                                            .From<Profile>()
                                                            .Where(p => p.Id == currentUserId)
                                                            .Get();
                        var currentUsername = currentUsernameResponse.Models.FirstOrDefault();

                        if (guessingUsername == currentUsername.Username)
                        {
                            Guess.IsEnabled = false;
                        }

                        string guessedText = $"{updatedData["username"]} adivinó la palabra!";
                        Guesses.Add(guessedText);
                    }
                    else
                    {
                        string guessText = $"{updatedData["username"]}: {updatedData["guess"]}";
                        Guesses.Add(guessText);
                    }

                    await Task.Delay(50);

                    if(Guesses.Count > 0 )
                    {
                        GuessesList.ScrollTo(Guesses[^1], position: ScrollToPosition.End, animate: true);
                    }
                });
            }
        });

        await _channelGuess.Subscribe();
        Debug.WriteLine("Suscrito a adivinanzas");

    }

    private async Task SubscribeToRoundUpdates()
    {
        _roundChannel = SupabaseClientService.SupabaseClient.Realtime
            .Channel("realtime", "public", "sessions");

        _roundChannel.AddPostgresChangeHandler(PostgresChangesOptions.ListenType.All, async (sender, change) =>
        {
            var jsonProperty = change.GetType().GetProperty("Json", BindingFlags.NonPublic | BindingFlags.Instance);
            string jsonString = jsonProperty?.GetValue(change)?.ToString();

            var json = JObject.Parse(jsonString);
            jsonString = json.ToString(Newtonsoft.Json.Formatting.Indented);

            var payloadData = json["payload"]?["data"];
            if (payloadData != null)
            {
                var updatedData = payloadData["record"];
                var sessionId = updatedData["id"]?.ToString();
                var newWord = updatedData["current_word"]?.ToString();
                var drawerUserId = updatedData["drawer_user_id"]?.ToString();
                var hasGameEnded = updatedData["has_game_ended"]?.ToString();
                Console.WriteLine($"Estado de game_ended: {hasGameEnded}");

                if (sessionId != _sessionId)
                {
                    Console.WriteLine("Evento ignorado: sesión no coincide.");
                    return;
                }

                Console.WriteLine("Cambio de ronda detectado:");
                Console.WriteLine($"Nueva palabra: {newWord}");
                Console.WriteLine($"Drawer: {drawerUserId}");

                if(hasGameEnded == "True")
                {
                    Console.WriteLine("Se acabó la partida");

                    Console.WriteLine($"Shell.Current: {(Shell.Current == null ? "null" : "OK")}");
                    try
                    {
                        await Shell.Current.GoToAsync("//MainPage");
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al navegar: {ex.Message}");
                    }
                }

                if (!string.IsNullOrEmpty(newWord))
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (drawerUserId == SupabaseClientService.SupabaseClient.Auth.CurrentUser.Id)
                        {
                            _drawingView.Lines.Clear();
                            var drawingId = updatedData["current_drawing_id"]?.ToString();

                            if (!string.IsNullOrEmpty(drawingId))
                            {
                                await Shell.Current.GoToAsync($"//DrawingPage?word={newWord}&sessionId={_sessionId}&drawingId={drawingId}");
                            }
                            else
                            {
                                Debug.WriteLine("No se encontró drawingId para la nueva ronda.");
                            }
                        }
                        else
                        {
                            _drawingView.Lines.Clear();
                            Guess.IsEnabled = true;
                            await Shell.Current.GoToAsync($"//GuessingPage?word={newWord}&sessionId={_sessionId}");
                        }
                    });
                }
            }
        });

        await _roundChannel.Subscribe();
        Debug.WriteLine("Suscrito a cambio de ronda");
    }

}