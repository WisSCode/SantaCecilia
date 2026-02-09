using frontend.Services;

namespace frontend.Pages;

public partial class ReportsPage : ContentPage
{
    private readonly ApiService _api;
    private List<ReportItem> reportItems = new();

    public ReportsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
        ReportWeekLabel.Text = $"Semana del {weekStart:dd 'de' MMMM 'de' yyyy}";
        await LoadReportAsync(weekStart.Date);
    }

    private async Task LoadReportAsync(DateTime weekStart)
    {
        try
        {
            var workTypes = await _api.GetWorkTypesAsync();
            var workedTimes = await _api.GetWorkedTimesAsync();

            var workTypeMap = workTypes.ToDictionary(wt => wt.Id, wt => wt);

            var weekEnd = weekStart.AddDays(7);
            var weekEntries = workedTimes
                .Where(wt => wt.Date >= weekStart && wt.Date < weekEnd)
                .ToList();

            reportItems = weekEntries
                .GroupBy(wt => wt.WorkTypeId)
                .Select(group =>
                {
                    var wt = workTypeMap.GetValueOrDefault(group.Key);
                    var name = wt?.Name ?? group.Key;
                    var rate = (decimal)(wt?.DefaultRate ?? 0);
                    var totalHours = group.Sum(e => e.MinutesWorked / 60m);
                    return new ReportItem(name, totalHours, rate);
                })
                .OrderByDescending(r => r.Hours)
                .ToList();

            ReportList.ItemsSource = reportItems;
            var totalHours = reportItems.Sum(r => r.Hours);
            var totalAmount = reportItems.Sum(r => r.Amount);
            TotalHoursLabel.Text = $"{totalHours:F1}h";
            TotalAmountLabel.Text = $"B/.{totalAmount:F2}";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo cargar el reporte: {ex.Message}", "OK");
        }
    }

    private class ReportItem
    {
        public ReportItem(string activityName, decimal hours, decimal rate)
        {
            ActivityName = activityName;
            Hours = hours;
            Rate = rate;
            Amount = hours * rate;
        }

        public string ActivityName { get; }
        public decimal Hours { get; }
        public decimal Rate { get; }
        public decimal Amount { get; }
    }
}
