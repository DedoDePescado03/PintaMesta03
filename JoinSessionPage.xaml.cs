namespace PintaMesta;
using PintaMesta.Models;
using System.Diagnostics;
using System.Xml;

public partial class JoinSessionPage : ContentPage
{
	public JoinSessionPage()
	{
		InitializeComponent();
	}

    private async Task AnimateButton(Button button)
    {
        await button.ScaleTo(1.1, 100, Easing.CubicIn);
        await button.ScaleTo(1.0, 100, Easing.CubicOut);
    }

    private static string GenerateUuid()
    {
        return Guid.NewGuid().ToString();
    }

    private async void JoinASession(object sender, EventArgs e)
	{
        var button = (Button)sender;
        await AnimateButton(button);

        if (SessionCode.Text == null)
        {
            await DisplayAlert("Error", "Ingresa un código", "Ok");
            return;
        }

        var client = SupabaseClientService.SupabaseClient;
        var user = client.Auth.CurrentUser;

        string inputCode = SessionCode.Text?.Trim().ToUpper();

        try
        {
            var response = await client.From<Session>().Where(s => s.Code == inputCode).Get();

            if (response == null)
            {
                await DisplayAlert("Error", $"No existe ninguna sesión con el código: {inputCode}", "Ok");
            }

            var session = response.Models.FirstOrDefault();

            var player = new SessionPlayer
            {
                Id = GenerateUuid(),
                SessionId = session.Id,
                UserId = user.Id,
                IsDrawer = false,
                Score = 0,
                JoinedAt = DateTime.Now,
            };

            await client.From<SessionPlayer>().Insert(player);
            await Shell.Current.GoToAsync($"LobbyPage?sessionId={session.Id}&code={session.Code}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al unir al jugador a la sesión: {ex}");
            await DisplayAlert("Error", "Ocurrió un error al intentar unirte a la sesión", "Ok");
        }

	}
}