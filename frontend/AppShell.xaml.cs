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
        Routing.RegisterRoute("newworker", typeof(NewWorkerPage));
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

        // Próximo sábado
        int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)now.DayOfWeek + 7) % 7;

        var nextSaturday = now.Date.AddDays(daysUntilSaturday);

        var paymentTime = nextSaturday.AddHours(15); // 3:00 PM

        // Si hoy es sábado y ya pasó las 3 PM → ir al siguiente sábado
        if (now >= paymentTime)
        {
            nextSaturday = nextSaturday.AddDays(7);
            paymentTime = nextSaturday.AddHours(15);
        }

        // Formato bonito en español
        var culture = new System.Globalization.CultureInfo("es-ES");
        var formattedDate = paymentTime.ToString("dddd dd 'de' MMMM h:mm tt", culture);

        NextPaymentLabel.Text = char.ToUpper(formattedDate[0]) + formattedDate.Substring(1);
    }

}