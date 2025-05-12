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

public partial class GuessingPage : ContentPage, IQueryAttributable
{
    private IOrientationService _orientationService;
    private System.Timers.Timer _timer;
    private string _sessionId;
    private string _lastDrawingId = null;
    private RealtimeChannel _channel;

    public GuessingPage()
	{
		InitializeComponent();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("sessionId"))
            _sessionId = query["sessionId"] as string;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _orientationService = App.Services.GetRequiredService<IOrientationService>();
        _orientationService.ForceLandscape();
        SubscribeToDrawingUpdates();
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _orientationService?.AllowOrientations();
        _channel.Unsubscribe();
    }

    private async void SubscribeToDrawingUpdates()
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
                Console.WriteLine("Updated User:");
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
            var response = await SupabaseClientService.SupabaseClient
                .From<DrawingData>()
                .Where(d => d.SessionId == _sessionId)
                .Get();

            var drawings = response.Models;

            if (drawings.Count > 0)
            {
                foreach (var drawing in drawings)
                {
                    if(drawing.IsCleared == true)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Drawing.Lines.Clear();
                        });
                    }
                    else
                    {
                        var lines = JsonSerializer.Deserialize<List<SerializedLine>>(drawing.Points);

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            foreach (var lineData in lines)
                            {
                                var line = new DrawingLine
                                {
                                    Points = new ObservableCollection<PointF>(lineData.Points.Select(p => new PointF(p.X, p.Y))),
                                    LineColor = Color.FromHex(lineData.Color),
                                    LineWidth = lineData.Width,
                                };

                                Drawing.Lines.Add(line);
                            }
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}