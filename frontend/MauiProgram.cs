using Microsoft.Extensions.Logging;
using frontend.Services;
using frontend.ViewModels;
using frontend.Configuration;
using frontend.Pages;

namespace frontend
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configurar HttpClient para el backend
            builder.Services.AddSingleton(sp => 
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(AppSettings.BackendUrl)
                };
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                return httpClient;
            });

            // Registrar servicios
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<SessionService>();
            builder.Services.AddSingleton<ApiService>();

            // Registrar ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();

            // Registrar páginas
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<WorkersPage>();
            builder.Services.AddTransient<TimeTrackingPage>();
            builder.Services.AddTransient<PayrollPage>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<UsersPage>();
            builder.Services.AddTransient<WorkTypesPage>();
            builder.Services.AddTransient<BatchesPage>();
            builder.Services.AddTransient<LogsPage>();
            builder.Services.AddTransient<NewEntryPage>();
            builder.Services.AddTransient<EditEntryPage>();
            builder.Services.AddTransient<NewWorkerPage>();
            builder.Services.AddTransient<NewBatchPage>();
            builder.Services.AddTransient<NewWorkTypePage>();
            builder.Services.AddTransient<EditBatchPage>();
            builder.Services.AddTransient<EditWorkTypePage>();
            builder.Services.AddTransient<EditUserPage>();
            builder.Services.AddTransient<ValidateUserPage>();
            builder.Services.AddTransient<WorkersHomePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
