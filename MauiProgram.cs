﻿using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using PintaMesta.Services;
using Plugin.Maui.Audio;

#if ANDROID
using PintaMesta.Platforms.Android;
#endif

#if IOS
using PintaMesta.Platforms.iOS;
#endif

namespace PintaMesta
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Jersey10-Regular.ttf", "Jersey10");
                });

            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if ANDROID
            builder.Services.AddSingleton<IOrientationService, OrientationService>();
#endif

#if IOS
            builder.Services.AddSingleton<IOrientationService, OrientationService>();
#endif

            var mauiApp = builder.Build();

            // Pass the service provider to App after building the MauiApp
            return mauiApp;
        }
    }
}
