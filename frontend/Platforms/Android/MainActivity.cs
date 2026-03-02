using Android.App;
using Android.Content.Res;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace frontend
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private const string BrandSystemBarColor = "#9EC9B4";

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ApplySystemBarsAppearance();
        }

        protected override void OnResume()
        {
            base.OnResume();
            ApplySystemBarsAppearance();
        }

        private void ApplySystemBarsAppearance()
        {
            if (Window is null)
                return;

            var insetsController = new WindowInsetsControllerCompat(Window, Window.DecorView);
            var useLightIcons = !IsNightModeActive();
            insetsController.AppearanceLightStatusBars = useLightIcons;
            insetsController.AppearanceLightNavigationBars = useLightIcons;

            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
            {
                var barColor = Android.Graphics.Color.ParseColor(BrandSystemBarColor);
                Window.SetStatusBarColor(barColor);
                Window.SetNavigationBarColor(barColor);
            }
        }

        private bool IsNightModeActive()
        {
            // Comprobamos el UiMode y nos aseguramos de no usar un valor obsoleto
            var uiMode = Resources?.Configuration?.UiMode ?? UiMode.NightNo;
            return (uiMode & UiMode.NightMask) == UiMode.NightYes;
        }
    }
}