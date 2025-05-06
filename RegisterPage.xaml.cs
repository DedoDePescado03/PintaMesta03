namespace PintaMesta;
using PintaMesta.Models;
using System.Diagnostics;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
	}

	private async void RegisterNewUser(System.Object sender, System.EventArgs e)
	{
		var user_name = usernameRegister.Text?.Trim();
		var email = emailRegister.Text?.Trim();
		var password = passwordRegister.Text?.Trim();

		if (string.IsNullOrEmpty(user_name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
		{
			await DisplayAlert("Error", "Por favor, llena todos lo campos", "Ok");
			return;
		}

		try
		{
			var supabase = SupabaseClientService.SupabaseClient;

			var session = await supabase.Auth.SignUp(email, password);

			if (session != null && session.User != null)
			{

				await DisplayAlert("Éxito", "Usuario registrado correctamente", "Ok");
                string createdUserId = session.User.Id;

                var profile = new Profile
				{
					Id = createdUserId,
					Username = user_name,
				};
				await supabase.From<Profile>().Insert(profile);

				await Navigation.PushAsync(new LoginPage());
			}
			else
			{
				await DisplayAlert("Error", "No se pudo registrar el usuario", "Ok");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "Ok");
		}
	}
}