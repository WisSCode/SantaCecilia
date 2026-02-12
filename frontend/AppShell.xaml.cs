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

        UpdateNextPaymentDate();
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

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Perfil", "Funcionalidad en desarrollo", "OK");
    }

    private void UpdateNextPaymentDate()
    {
        var now = DateTime.Now;

        // Calculate next Saturday
        int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)now.DayOfWeek + 7) % 7;
        var nextSaturday = now.Date.AddDays(daysUntilSaturday);

        // Set payment time at 3:00 PM
        var paymentTime = nextSaturday.AddHours(15);

        // If today is Saturday and it's already past 3 PM, move to next Saturday
        if (now >= paymentTime)
        {
            nextSaturday = nextSaturday.AddDays(7);
            paymentTime = nextSaturday.AddHours(15);
        }

        // Format date in Spanish locale
        var culture = new System.Globalization.CultureInfo("es-ES");
        var formattedDate = paymentTime.ToString("ddd dd 'de' MMM", culture);

        NextPaymentLabel.Text = char.ToUpper(formattedDate[0]) + formattedDate.Substring(1);
    }
}