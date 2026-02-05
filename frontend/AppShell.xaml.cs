//using System.IO;

//namespace frontend
//{
//    public partial class AppShell : Shell
//    {
//        private static readonly string logFilePath = Path.Combine(
//            Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
//            "frontend_log.txt");

//        public AppShell()
//        {
//            try
//            {
//                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] AppShell constructor started\n");
//                InitializeComponent();
//                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] AppShell InitializeComponent OK\n");
//                Routing.RegisterRoute("newtimeentry", typeof(NewEntryPage));
//                Routing.RegisterRoute("login", typeof(LoginPage));
//                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] Routes registered\n");
//            }
//            catch (Exception ex)
//            {
//                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] EXCEPTION in AppShell constructor:\n{ex}\n");
//                throw;
//            }
//        }

//        protected override async void OnAppearing()
//        {
//            base.OnAppearing();
//            File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] AppShell OnAppearing - showing dashboard\n");

//            try
//            {
//                // Show login page as modal
//                await Shell.Current.GoToAsync("login");
//                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] Navigation to login OK\n");
//            }
//            catch (Exception ex)
//            {
//                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss}] Navigation error:\n");
//                File.AppendAllText(logFilePath, $"  Message: {ex.Message}\n");
//                File.AppendAllText(logFilePath, $"  StackTrace: {ex.StackTrace}\n");
//                if (ex.InnerException != null)
//                {
//                    File.AppendAllText(logFilePath, $"  InnerException: {ex.InnerException.Message}\n");
//                    File.AppendAllText(logFilePath, $"  InnerStackTrace: {ex.InnerException.StackTrace}\n");
//                }
//                // If navigation fails, just stay on main shell
//            }
//        }
//    }
//}
using System;
using Microsoft.Maui.Controls;

namespace frontend
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OnEditProfileClicked(object sender, EventArgs e)
        {
            await DisplayAlertAsync("Perfil", "Edicion de perfil en progreso.", "OK");
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            Application.Current!.MainPage = new LoginPage();
        }
    }
}
    