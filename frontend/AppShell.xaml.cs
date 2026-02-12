using frontend.Pages;
using frontend.Services;

namespace frontend;

public partial class AppShell : Shell
{
    private readonly SessionService _sessionService;

    public AppShell(SessionService sessionService)
    {
        InitializeComponent();

        _sessionService = sessionService;

        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("newtimeentry", typeof(NewEntryPage));
        Routing.RegisterRoute("edittimeentry", typeof(EditEntryPage));
        Routing.RegisterRoute("newworker", typeof(NewWorkerPage));
        Routing.RegisterRoute("editworker", typeof(EditWorkerPage));
        Routing.RegisterRoute("newbatch", typeof(NewBatchPage));
        Routing.RegisterRoute("newworktype", typeof(NewWorkTypePage));
        Routing.RegisterRoute("editbatch", typeof(EditBatchPage));
        Routing.RegisterRoute("editworktype", typeof(EditWorkTypePage));
        Routing.RegisterRoute("edituser", typeof(EditUserPage));

        ConfigureShellForAuthState(false);
    }

    public void ConfigureShellForAuthState(bool isLoggedIn)
    {
        FlyoutBehavior = isLoggedIn ? FlyoutBehavior.Flyout : FlyoutBehavior.Disabled;

        foreach (var item in Items.OfType<FlyoutItem>())
        {
            item.IsVisible = isLoggedIn;
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _sessionService.LogoutAsync();
        if (Application.Current is App app)
            await app.GoToLoginAsync();
    }
}