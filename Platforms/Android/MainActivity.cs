using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using PintaMesta.Services;

namespace PintaMesta
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
        ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Ocultar barra de estado y navegación
            var decorView = Window.DecorView;
            decorView.SystemUiVisibility = (StatusBarVisibility)(
                SystemUiFlags.ImmersiveSticky |
                SystemUiFlags.Fullscreen |
                SystemUiFlags.HideNavigation |
                SystemUiFlags.LayoutFullscreen |
                SystemUiFlags.LayoutHideNavigation |
                SystemUiFlags.LayoutStable
            );
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Volver a ocultar al regresar de otra app
            var decorView = Window.DecorView;
            decorView.SystemUiVisibility = (StatusBarVisibility)(
                SystemUiFlags.ImmersiveSticky |
                SystemUiFlags.Fullscreen |
                SystemUiFlags.HideNavigation |
                SystemUiFlags.LayoutFullscreen |
                SystemUiFlags.LayoutHideNavigation |
                SystemUiFlags.LayoutStable
            );
        }
    }
}
