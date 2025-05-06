namespace PintaMesta;

using PintaMesta.Models;
using Supabase;
using System.Diagnostics;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class LobbyPage : ContentPage, IQueryAttributable
{
    public LobbyPage()
    {
        InitializeComponent();
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("sessionId"))
        {
            var sessionId = query["sessionId"] as string;

            var client = SupabaseClientService.SupabaseClient;

            try
            {
                var sessionResponse = await client.From<Session>().Where(s => s.Id == sessionId).Get();
                var session = sessionResponse.Models.FirstOrDefault();

                var hostUsernameResponse = await client.From<Profile>().Where(s => s.Id == session.HostUserId).Get();
                var hostUsername = hostUsernameResponse.Models.FirstOrDefault();

                if (session != null)
                {
                    SessionCodeLabel.Text = $"C�digo de sesi�n: {session.Code}";
                    SessionName.Text = $"Sesi�n de {hostUsername.Username}";
                    // Agrega m�s elementos aqu� si quieres mostrar la palabra actual, el host, etc.
                }
                else
                {
                    await DisplayAlert("Error", "Sesi�n no encontrada", "Ok");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar la sesi�n: {ex}");
                await DisplayAlert("Error", "No se pudo cargar la sesi�n", "Ok");
            }
        }
    }
}