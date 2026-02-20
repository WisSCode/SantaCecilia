using frontend.Pages;
using frontend.Services;

namespace frontend;

public partial class AppShell : Shell
{
    private readonly SessionService _sessionService;

    private static readonly HashSet<string> AdminRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "dashboard", "dashboardContent",
        "timetracking", "timetrackingContent",
        "workers", "workersContent",
        "payroll", "payrollContent",
        "reports", "reportsContent",
        "batches", "batchesContent",
        "worktypes", "worktypesContent",
        "logs", "logsContent",
        "users", "usersContent",
        "newtimeentry", "newworker", "newbatch", "newworktype",
        "editbatch", "editworktype", "edituser", "validateuser"
    };

    public AppShell(SessionService sessionService)
    {
        _sessionService = sessionService;
        InitializeComponent();

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
        Routing.RegisterRoute("validateuser", typeof(ValidateUserPage));

        ConfigureShellForAuthState(false);

        UpdateNextPaymentDate();
    }

    public void ConfigureShellForAuthState(bool isLoggedIn)
    {
        foreach (var item in Items.OfType<FlyoutItem>())
        {
            item.IsVisible = false;
        }

        if (!isLoggedIn)
        {
            FlyoutBehavior = FlyoutBehavior.Disabled;
            return;
        }

        bool isAdmin = _sessionService.HasRole("admin");

        FlyoutBehavior = isAdmin ? FlyoutBehavior.Flyout : FlyoutBehavior.Disabled;

        // Admin items
        DashboardItem.IsVisible = isAdmin;
        TimeTrackingItem.IsVisible = isAdmin;
        WorkersItem.IsVisible = isAdmin;
        PayrollItem.IsVisible = isAdmin;
        ReportsItem.IsVisible = isAdmin;
        BatchesItem.IsVisible = isAdmin;
        WorkTypesItem.IsVisible = isAdmin;
        UsersItem.IsVisible = isAdmin;
        LogsItem.IsVisible = isAdmin;

        // Worker items
        WorkerHomeItem.IsVisible = !isAdmin;
    }

    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);

        if (!_sessionService.IsLoggedIn)
            return;

        if (_sessionService.HasRole("admin"))
            return;

        var target = args.Target?.Location?.OriginalString ?? "";
        var segments = target.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            if (AdminRoutes.Contains(segment))
            {
                args.Cancel();
                return;
            }
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