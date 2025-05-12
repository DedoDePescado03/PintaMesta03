using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Views;

namespace PintaMesta.Platforms.Android
{
    public static class PlatformUtils
    {
        public static void OcultarBarraDeEstado()
        {
            var decorView = Platform.CurrentActivity?.Window?.DecorView;
            if (decorView != null)
            {
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
}
