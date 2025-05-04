using Foundation;
using PintaMesta.Services;
using UIKit;

namespace PintaMesta.Platforms.iOS
{
    public class OrientationService : IOrientationService
    {
        public void ForceLandscape()
        {
            UIDevice.CurrentDevice.SetValueForKey(
                NSNumber.FromInt32((int)UIInterfaceOrientation.LandscapeLeft),
                new NSString("orientation")
            );
        }

        public void AllowOrientations()
        {
            UIDevice.CurrentDevice.SetValueForKey(
                NSNumber.FromInt32((int)UIInterfaceOrientation.Portrait),
                new NSString("orientation")
            );
        }
    }
}
