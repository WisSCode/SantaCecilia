using System.Linq;
using frontend.Pages;
using frontend.Services;

namespace frontend;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(ResolveLoginPage()));
    }

    public LoginPage ResolveLoginPage()
    {
        return _services.GetRequiredService<LoginPage>();
    }

    public async Task GoToDashboardAsync()
    {
        var sessionService = _services.GetRequiredService<SessionService>();
        var shell = new AppShell(sessionService);
        shell.ConfigureShellForAuthState(true);
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window == null)
            return;
        window.Page = shell;
        await Shell.Current.GoToAsync("//dashboard");
    }

    public async Task GoToLoginAsync()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window == null)
            return;
        window.Page = new NavigationPage(ResolveLoginPage());
        await Task.CompletedTask;
    }
}