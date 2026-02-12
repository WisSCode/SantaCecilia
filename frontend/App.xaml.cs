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

    public async Task GoToShellAsync()
    {
        var sessionService = _services.GetRequiredService<SessionService>();
        var shell = new AppShell(sessionService);
        shell.ConfigureShellForAuthState(true);
        MainPage = shell;

        if (sessionService.HasRole("admin"))
            await Shell.Current.GoToAsync("//dashboard");
        else
            await Shell.Current.GoToAsync("//workershome");
    }

    public async Task GoToLoginAsync()
    {
        MainPage = new NavigationPage(ResolveLoginPage());
        await Task.CompletedTask;
    }
}