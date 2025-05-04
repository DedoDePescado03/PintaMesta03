namespace PintaMesta;
using PintaMesta.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

public partial class DrawingPage : ContentPage
{
    private IOrientationService _orientationService;

    public DrawingPage()
    {
        InitializeComponent();
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
                _ => DrawBoard.LineColor
            };
        }
    }
}
