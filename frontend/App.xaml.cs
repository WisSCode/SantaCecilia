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
        var window = Windows.FirstOrDefault();
        if (window is not null)
            window.Page = shell;

        if (sessionService.HasRole("admin"))
            await shell.GoToAsync("//dashboard");
        else
            await shell.GoToAsync("//workershome");
    }

    public async Task GoToLoginAsync()
    {
        var window = Windows.FirstOrDefault();
        if (window is not null)
            window.Page = new NavigationPage(ResolveLoginPage());
        await Task.CompletedTask;
    }
}