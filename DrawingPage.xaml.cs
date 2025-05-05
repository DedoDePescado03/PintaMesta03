namespace PintaMesta;
using PintaMesta.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;

public partial class DrawingPage : ContentPage, IQueryAttributable
{
    private IOrientationService _orientationService;

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

    private void ClearDrawingView(System.Object sender, System.EventArgs e)
    {
        DrawBoard.Lines.Clear();
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
}
