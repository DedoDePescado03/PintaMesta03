namespace PintaMesta;
using PintaMesta.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using PintaMesta.Models;
using System.Linq;
using System.Text.Json;

public partial class DrawingPage : ContentPage, IQueryAttributable
{
    private IOrientationService _orientationService;
    private System.Timers.Timer _timer;
    private string _sessionId;
    private string _drawingRecordId;

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
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _orientationService = App.Services.GetRequiredService<IOrientationService>();
        _orientationService.ForceLandscape();
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
