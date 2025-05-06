using PintaMesta.Services;
using Android.Content.PM;
using Microsoft.Maui.ApplicationModel;

namespace PintaMesta.Platforms.Android
{
    public class OrientationService : IOrientationService
    {
        public void ForceLandscape()
        {
            var activity = Platform.CurrentActivity;
            activity.RequestedOrientation = ScreenOrientation.SensorLandscape;
        }

        public void AllowOrientations()
        {
            var activity = Platform.CurrentActivity;
            activity.RequestedOrientation = ScreenOrientation.Unspecified;
        }

        public void ForcePortrait()
        {
            var activity = Platform.CurrentActivity;
            activity.RequestedOrientation = ScreenOrientation.Portrait;
        }

    }
}
