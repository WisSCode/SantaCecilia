using System.Globalization;
using frontend.Helpers;
using frontend.Models;
using frontend.Services;

namespace frontend.Pages;

public partial class DashboardPage : ContentPage
{
    private enum SortDirection { Ascending, Descending }

    private readonly ApiService _api;
    private List<TimeEntry> _recentActivities = new();
    private SortDirection _workerSortDirection = SortDirection.Descending;
    private SortDirection _activitySortDirection = SortDirection.Ascending;
    private SortDirection _batchSortDirection = SortDirection.Ascending;
    private SortDirection _rateSortDirection = SortDirection.Ascending;
    private SortDirection _hoursSortDirection = SortDirection.Ascending;
    private SortDirection _dateSortDirection = SortDirection.Ascending;

    public DashboardPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DateLabel.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            var workers = await _api.GetWorkersAsync();
            var workTypes = await _api.GetWorkTypesAsync();
            var batches = await _api.GetBatchesAsync();
            var workedTimes = await _api.GetWorkedTimesAsync();
            var payrolls = await _api.GetPayrollsAsync();
            var users = await _api.GetUsersAsync();

            var workerMap = workers.ToDictionary(w => w.Id, w => $"{w.Name} {w.LastName}");
            var workTypeMap = workTypes.ToDictionary(wt => wt.Id, wt => wt.Name);
            var workTypeRateMap = workTypes.ToDictionary(wt => wt.Id, wt => wt.DefaultRate);
            var batchMap = batches.ToDictionary(b => b.Id, b => b.Name);

            var (weekStart, weekEnd) = WeekHelper.GetWeekRange(DateTime.Now);
            var lastWeekStart = weekStart.AddDays(-7);

            // --- Stats ---

            // Trabajadores activos
            var activeWorkers = workers.Count(w => w.Active);
            WorkersCount.Text = activeWorkers.ToString();

            // New workers this week (approximate: all active as we can't track creation date)
            // Just show total active for
            // 
            //WorkersNewBadge.Text = $"+{activeWorkers}";

            // Horas trabajadas (this week)
            var weekEntries = workedTimes.Where(wt => wt.Date.Date >= weekStart && wt.Date.Date < weekEnd).ToList();
            var totalMinutesWeek = weekEntries.Sum(wt => wt.MinutesWorked);
            var totalHoursWeek = totalMinutesWeek / 60m;
            HoursCount.Text = $"{totalHoursWeek:N0}";

            // Daily average
            var today = DateTime.Today;
            var daysInWeek = Math.Max(1, (int)(today < weekEnd ? (today - weekStart).TotalDays + 1 : 7));
            var dailyAvg = totalMinutesWeek / 60m / daysInWeek;
            //HoursDailyBadge.Text = $"{dailyAvg:F0}";

            // Nomina semanal
            var weekPayrolls = payrolls.Where(p => p.WeekStart >= weekStart && p.WeekStart < weekEnd).ToList();
            var totalPayroll = weekPayrolls.Sum(p => p.GrossAmount);
            PayrollAmount.Text = $"B/.{totalPayroll:N2}";

            // Actividades (total worked times this week vs last week)
            var thisWeekCount = weekEntries.Count;
            var lastWeekEntries = workedTimes.Where(wt => wt.Date.Date >= lastWeekStart && wt.Date.Date < weekStart).ToList();
            var diff = thisWeekCount - lastWeekEntries.Count;
            ActivitiesCount.Text = thisWeekCount.ToString();
            //ActivitiesNewBadge.Text = diff >= 0 ? $"+{diff}" : $"{diff}";

            // --- Alerts ---

            // Days until Saturday 3PM (payroll deadline)
            var daysOfWeek = (int)today.DayOfWeek;
            var daysUntilSaturday = ((int)DayOfWeek.Saturday - daysOfWeek + 7) % 7;
            if (daysUntilSaturday == 0) daysUntilSaturday = 0; // Today is Saturday
            PayrollAlertLabel.Text = $"Faltan {daysUntilSaturday} días para el cierre de nómina semanal";

            // Users pending validation
            var pendingUsers = users.Count(u => !u.Validated);
            ValidationAlertLabel.Text = $"{pendingUsers} usuarios pendientes de validacion";
            ValidationAlertBorder.IsVisible = pendingUsers > 0;

            // --- Recent activities (last 5 of current week) ---
            var recent = weekEntries
                .OrderByDescending(wt => wt.Date)
                .Take(5)
                .Select(wt => new TimeEntry
                {
                    WorkerId = wt.WorkerId,
                    WorkerName = workerMap.GetValueOrDefault(wt.WorkerId, wt.WorkerId),
                    ActivityName = workTypeMap.GetValueOrDefault(wt.WorkTypeId, wt.WorkTypeId),
                    Lote = batchMap.GetValueOrDefault(wt.BatchId, wt.BatchId),
                    Rate = (decimal)workTypeRateMap.GetValueOrDefault(wt.WorkTypeId, 0),
                    Hours = wt.MinutesWorked / 60,
                    Minutes = wt.MinutesWorked % 60,
                    Date = wt.Date
                })
                .ToList();

            _recentActivities = recent;
            RecentActivitiesCollection.ItemsSource = _recentActivities;
            
            // Reset sort directions and headers
            _workerSortDirection = SortDirection.Descending;
            _activitySortDirection = SortDirection.Ascending;
            _batchSortDirection = SortDirection.Ascending;
            _rateSortDirection = SortDirection.Ascending;
            _hoursSortDirection = SortDirection.Ascending;
            _dateSortDirection = SortDirection.Ascending;
            
            HeaderWorker.Text = "TRABAJADOR \u2193";
            HeaderActivity.Text = "ACTIVIDAD";
            HeaderBatch.Text = "LOTE";
            HeaderRate.Text = "TARIFA";
            HeaderHours.Text = "HORAS";
            HeaderDate.Text = "FECHA";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los datos: {ex.Message}", "OK");
        }
    }

    private void OnSortByWorker(object sender, TappedEventArgs e)
    {
        _workerSortDirection = _workerSortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

        var sorted = _workerSortDirection == SortDirection.Ascending
            ? _recentActivities.OrderBy(x => x.WorkerName).ToList()
            : _recentActivities.OrderByDescending(x => x.WorkerName).ToList();

        _recentActivities = sorted;
        RecentActivitiesCollection.ItemsSource = null;
        RecentActivitiesCollection.ItemsSource = _recentActivities;

        HeaderWorker.Text = _workerSortDirection == SortDirection.Ascending ? "TRABAJADOR \u2191" : "TRABAJADOR \u2193";
        HeaderActivity.Text = "ACTIVIDAD";
        HeaderBatch.Text = "LOTE";
        HeaderRate.Text = "TARIFA";
        HeaderHours.Text = "HORAS";
        HeaderDate.Text = "FECHA";
    }

    private void OnSortByActivity(object sender, TappedEventArgs e)
    {
        _activitySortDirection = _activitySortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

        var sorted = _activitySortDirection == SortDirection.Ascending
            ? _recentActivities.OrderBy(x => x.ActivityName).ToList()
            : _recentActivities.OrderByDescending(x => x.ActivityName).ToList();

        _recentActivities = sorted;
        RecentActivitiesCollection.ItemsSource = null;
        RecentActivitiesCollection.ItemsSource = _recentActivities;

        HeaderWorker.Text = "TRABAJADOR";
        HeaderActivity.Text = _activitySortDirection == SortDirection.Ascending ? "ACTIVIDAD \u2191" : "ACTIVIDAD \u2193";
        HeaderBatch.Text = "LOTE";
        HeaderRate.Text = "TARIFA";
        HeaderHours.Text = "HORAS";
        HeaderDate.Text = "FECHA";
    }

    private void OnSortByBatch(object sender, TappedEventArgs e)
    {
        _batchSortDirection = _batchSortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

        var sorted = _batchSortDirection == SortDirection.Ascending
            ? _recentActivities.OrderBy(x => x.Lote).ToList()
            : _recentActivities.OrderByDescending(x => x.Lote).ToList();

        _recentActivities = sorted;
        RecentActivitiesCollection.ItemsSource = null;
        RecentActivitiesCollection.ItemsSource = _recentActivities;

        HeaderWorker.Text = "TRABAJADOR";
        HeaderActivity.Text = "ACTIVIDAD";
        HeaderBatch.Text = _batchSortDirection == SortDirection.Ascending ? "LOTE \u2191" : "LOTE \u2193";
        HeaderRate.Text = "TARIFA";
        HeaderHours.Text = "HORAS";
        HeaderDate.Text = "FECHA";
    }

    private void OnSortByRate(object sender, TappedEventArgs e)
    {
        _rateSortDirection = _rateSortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

        var sorted = _rateSortDirection == SortDirection.Ascending
            ? _recentActivities.OrderBy(x => x.Rate).ToList()
            : _recentActivities.OrderByDescending(x => x.Rate).ToList();

        _recentActivities = sorted;
        RecentActivitiesCollection.ItemsSource = null;
        RecentActivitiesCollection.ItemsSource = _recentActivities;

        HeaderWorker.Text = "TRABAJADOR";
        HeaderActivity.Text = "ACTIVIDAD";
        HeaderBatch.Text = "LOTE";
        HeaderRate.Text = _rateSortDirection == SortDirection.Ascending ? "TARIFA \u2191" : "TARIFA \u2193";
        HeaderHours.Text = "HORAS";
        HeaderDate.Text = "FECHA";
    }

    private void OnSortByHours(object sender, TappedEventArgs e)
    {
        _hoursSortDirection = _hoursSortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

        var sorted = _hoursSortDirection == SortDirection.Ascending
            ? _recentActivities.OrderBy(x => x.Hours).ToList()
            : _recentActivities.OrderByDescending(x => x.Hours).ToList();

        _recentActivities = sorted;
        RecentActivitiesCollection.ItemsSource = null;
        RecentActivitiesCollection.ItemsSource = _recentActivities;

        HeaderWorker.Text = "TRABAJADOR";
        HeaderActivity.Text = "ACTIVIDAD";
        HeaderBatch.Text = "LOTE";
        HeaderRate.Text = "TARIFA";
        HeaderHours.Text = _hoursSortDirection == SortDirection.Ascending ? "HORAS \u2191" : "HORAS \u2193";
        HeaderDate.Text = "FECHA";
    }

    private void OnSortByDate(object sender, TappedEventArgs e)
    {
        _dateSortDirection = _dateSortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

        var sorted = _dateSortDirection == SortDirection.Ascending
            ? _recentActivities.OrderBy(x => x.Date).ToList()
            : _recentActivities.OrderByDescending(x => x.Date).ToList();

        _recentActivities = sorted;
        RecentActivitiesCollection.ItemsSource = null;
        RecentActivitiesCollection.ItemsSource = _recentActivities;

        HeaderWorker.Text = "TRABAJADOR";
        HeaderActivity.Text = "ACTIVIDAD";
        HeaderBatch.Text = "LOTE";
        HeaderRate.Text = "TARIFA";
        HeaderHours.Text = "HORAS";
        HeaderDate.Text = _dateSortDirection == SortDirection.Ascending ? "FECHA \u2191" : "FECHA \u2193";
    }

    private async void OnViewAllTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//timetracking");
    }
}
