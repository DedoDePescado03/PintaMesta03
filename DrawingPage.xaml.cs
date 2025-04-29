namespace PintaMesta;

public partial class DrawingPage : ContentPage
{
	public DrawingPage()
	{
		InitializeComponent();
	}

	private void ClearDrawingView(System.Object sender, System.EventArgs e)
	{
		DrawBoard.Lines.Clear();
	}
}