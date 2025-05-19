namespace PintaMesta;
using PintaMesta.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using PintaMesta.Models;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;

public partial class DrawingPage : ContentPage, IQueryAttributable
{
    private IOrientationService _orientationService;
    private System.Timers.Timer _timer;
    private string _sessionId;
    private string _drawingRecordId;

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

    public DrawingPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("word", out var wordObj) && wordObj is string word)
        {
            CurrentWordLabel.Text = $"Dibuja: {Uri.UnescapeDataString(word)}";
        }
        else
        {
            CurrentWordLabel.Text = "No se recibió una palabra.";
        }
        if (query.ContainsKey("sessionId"))
        {
            _sessionId = query["sessionId"] as string;
        }
        if (query.ContainsKey("drawingId"))
        {
            _drawingRecordId = query["drawingId"] as string;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _orientationService = App.Services.GetRequiredService<IOrientationService>();
        _orientationService.ForceLandscape();
        StartRoundTimer();
    }

    private async void StartRoundTimer()
    {
        await Task.Delay(TimeSpan.FromMinutes(1));
        await EndRound();
    }

    private async Task EndRound()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            DrawBoard.Lines.Clear();
        });
        await StartNextRound();
    }

        private async Task StartNextRound()
        {
            var client = SupabaseClientService.SupabaseClient;

            var playersResponse = await client
                .From<SessionPlayer>()
                .Where(sp => sp.SessionId == _sessionId)
                .Order("joined_at", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            var players = playersResponse.Models;
            if (players.Count == 0) return;

            var sessionResponse = await client
                .From<Session>()
                .Where(s => s.Id == _sessionId)
                .Get();

            var session = sessionResponse.Models.FirstOrDefault();
            if (session == null) return;

            int currentDrawerIndex = players.FindIndex(p => p.UserId == session.DrawerUserId);
            int nextDrawerIndex = (currentDrawerIndex + 1) % players.Count;
            var nextDrawer = players[nextDrawerIndex];
            var increasedRound = session.RoundNumber + 1;

            if (increasedRound > players.Count)
            {
                Console.WriteLine("Se terminó la partida");
                //Terminar partida, ya todos dibujaron
                await client
                      .From<Session>()
                      .Where(s => s.Id == _sessionId)
                      .Set(s => s.HasGameEnded, true)
                      .Update();

            await Shell.Current.GoToAsync("//MainPage");
            return;
            }

            foreach (var player in players)
            {
                player.IsDrawer = (player.UserId == nextDrawer.UserId);
                await client
                    .From<SessionPlayer>()
                    .Where(sp => sp.Id == player.Id)
                    .Set(sp => sp.IsDrawer, player.IsDrawer)
                    .Update();
            }

            string newWord = GetRandomWord();
            string newDrawingId = Guid.NewGuid().ToString();
            _drawingRecordId = newDrawingId;

            // Guarda nuevo dibujo vacío en Supabase
            await client
                .From<DrawingData>()
                .Insert(new DrawingData
                {
                    Id = newDrawingId,
                    SessionId = _sessionId,
                    Points = "[]",
                    CreatedAt = DateTime.UtcNow,
                    IsCleared = false
                });

            await client
                .From<Session>()
                .Where(s => s.Id == _sessionId)
                .Set(s => s.DrawerUserId, nextDrawer.UserId)
                .Set(s => s.CurrentWord, newWord)
                .Set(s => s.RoundNumber, increasedRound)
                .Set(s => s.CurrentDrawingId, newDrawingId)
                .Update();

            ClearDrawing();


            Debug.WriteLine($"Nueva ronda: {newWord}, dibuja {nextDrawer.UserId}");
                await Shell.Current.GoToAsync("//GuessingPage", true, new Dictionary<string, object>
                {
                    { "sessionId", _sessionId },
                });
        }

    private async void ClearDrawing()
    {
        DrawBoard.Lines.Clear();

        var drawingUpdate = new DrawingData
        {
            Id = _drawingRecordId,
            SessionId = _sessionId,
            Points = "[]",
            IsCleared = true,
            LineColor = DrawBoard.LineColor.ToHex(),
            LineWidth = DrawBoard.LineWidth,
            CreatedAt = DateTime.UtcNow
        };

        await SupabaseClientService.SupabaseClient
            .From<DrawingData>()
                .Where(d => d.Id == _drawingRecordId)
                .Update(drawingUpdate);
    }

    private string GetRandomWord()
    {
        var random = new Random();
        int index = random.Next(WordList.Count);
        return WordList[index];
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _orientationService?.AllowOrientations();
    }

    private async void ClearDrawingView(object sender, EventArgs e)
    {
        DrawBoard.Lines.Clear();

        var drawingUpdate = new DrawingData
        {
            Id = _drawingRecordId,
            SessionId = _sessionId,
            Points = "[]",
            IsCleared = true,
            LineColor = DrawBoard.LineColor.ToHex(),
            LineWidth = DrawBoard.LineWidth,
            CreatedAt = DateTime.UtcNow
        };

        await SupabaseClientService.SupabaseClient
            .From<DrawingData>()
            .Where(d => d.Id == _drawingRecordId)
            .Update(drawingUpdate);
    }

    private void ChangeColor(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var colorName = button.CommandParameter.ToString();
            DrawBoard.LineColor = colorName switch
            {
                "Red" => Colors.Red,
                "Green" => Colors.Green,
                "Blue" => Colors.Blue,
                "Yellow" => Colors.Yellow,
                "Black" => Colors.Black,
                "Purple" => Colors.Purple,
                "Orange" => Colors.Orange,
                "Pink" => Colors.Pink,
                "Brown" => Colors.Brown,
                _ => DrawBoard.LineColor
            };
        }
    }

    private void LineWidthSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (DrawBoard != null)
        {
            DrawBoard.LineWidth = (float)e.NewValue;

            if (LineWidthLabel != null)
                LineWidthLabel.Text = $"Grosor de linea: {e.NewValue:F1}";
        }
    }

    private async void OnDrawn(object sender, EventArgs e)
    {
        await SaveDrawing();
    }

    private async Task SaveDrawing()
    {
        var allLines = DrawBoard.Lines.Select(line => new SerializedLine
        {
            Points = line.Points.Select(p => new PointData { X = p.X, Y = p.Y }).ToList(),
            Color = line.LineColor.ToHex(),
            Width = line.LineWidth
        }).ToList();

        var serialized = JsonSerializer.Serialize(allLines);

        if (string.IsNullOrEmpty(_drawingRecordId))
        {
            _drawingRecordId = Guid.NewGuid().ToString();
            var drawing = new DrawingData
            {
                Id = _drawingRecordId,
                SessionId = _sessionId,
                Points = serialized,
                CreatedAt = DateTime.UtcNow
            };

            await SupabaseClientService.SupabaseClient
                .From<DrawingData>()
                .Insert(drawing);

            await SupabaseClientService.SupabaseClient
                .From<Session>()
                .Where(s => s.Id == _sessionId)
                .Set(s => s.CurrentDrawingId, _drawingRecordId)
                .Update();
        }
        else
        {
            var drawingUpdate = new DrawingData
            {
                Id = _drawingRecordId,
                SessionId = _sessionId,
                Points = serialized,
                IsCleared = false,
                CreatedAt = DateTime.UtcNow
            };

            await SupabaseClientService.SupabaseClient
                .From<DrawingData>()
                .Where(d => d.Id == _drawingRecordId)
                .Update(drawingUpdate);
        }
    }

}
