using Supabase.Realtime.Channel;

namespace PintaMesta;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

	private async void GoToRegister(object sender,EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(RegisterPage));
	}

    private async void LoginUser(System.Object sender, System.EventArgs e)
    {
		var email = emailLogin.Text?.Trim();
		var password = passwordLogin.Text?.Trim();

		if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) 
		{
			await DisplayAlert("Error", "Por favor, llena todos los campos", "Ok");
			return;
		}

		try
		{
			var supabase = SupabaseClientService.SupabaseClient;

			var session = await supabase.Auth.SignIn(email, password);

			if(session != null && session.User != null)
			{
				await Shell.Current.GoToAsync("///MainPage");
			}
			else
			{
				await DisplayAlert("Error", "Correo o contraseña incorrecta", "Ok");
				return;
			}
		}
		catch(Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "Ok");
		}
    }
}