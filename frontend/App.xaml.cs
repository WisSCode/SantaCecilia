using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace frontend
{
    public partial class App : Application
    {
        private static readonly string logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
            "frontend_log.txt");

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.WriteLine("Excepción no capturada: " + e.ExceptionObject.ToString());
            };
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new LoginPage());
            return window;
        }
    }
}