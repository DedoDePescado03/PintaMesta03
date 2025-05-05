namespace PintaMesta;
using Supabase;
using System.Text.Json.Serialization;

public partial class LobbyPage : ContentPage, IQueryAttributable
{
	public LobbyPage()
	{
		InitializeComponent();
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.ContainsKey("code"))
		{
            string code = query["code"] as string;
			SessionCodeLabel.Text = $"El código de la sesión es: {code}";
		}
	}
}