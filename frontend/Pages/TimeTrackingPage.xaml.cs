using frontend.Models;
using frontend.Services;

namespace frontend.Pages;

public partial class TimeTrackingPage : ContentPage
{
    private readonly ApiService _api;
    private List<TimeEntry> allEntries = new();
    private Dictionary<string, string> workerMap = new();
    private Dictionary<string, string> workerIdentificationMap = new();
    private Dictionary<string, string> workTypeMap = new();
    private Dictionary<string, double> workTypeRateMap = new();
    private Dictionary<string, string> batchMap = new();

    public TimeTrackingPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        EventBus.NewEntrySaved += OnNewEntrySaved;
        await LoadEntriesAsync();
    }

    protected override void OnDisappearing()
    {
        EventBus.NewEntrySaved -= OnNewEntrySaved;
        base.OnDisappearing();
    }

    private async Task LoadEntriesAsync()
    {
        try
        {
            var workers = await _api.GetWorkersAsync();
            var workTypes = await _api.GetWorkTypesAsync();
            var batches = await _api.GetBatchesAsync();
            var workedTimes = await _api.GetWorkedTimesAsync();

            workerMap = workers.ToDictionary(w => w.Id, w => $"{w.Name} {w.LastName}");
            workerIdentificationMap = workers.ToDictionary(w => w.Id, w => w.Identification ?? string.Empty);
            workTypeMap = workTypes.ToDictionary(wt => wt.Id, wt => wt.Name);
            workTypeRateMap = workTypes.ToDictionary(wt => wt.Id, wt => wt.DefaultRate);
            batchMap = batches.ToDictionary(b => b.Id, b => b.Name);

            allEntries = workedTimes
                .OrderByDescending(wt => wt.Date)
                .Select(wt => new TimeEntry
                {
                    Id = wt.Id,
                    WorkerId = wt.WorkerId,
                    WorkerName = workerMap.GetValueOrDefault(wt.WorkerId, wt.WorkerId),
                    WorkerIdentification = workerIdentificationMap.GetValueOrDefault(wt.WorkerId, string.Empty),
                    ActivityName = workTypeMap.GetValueOrDefault(wt.WorkTypeId, wt.WorkTypeId),
                    Lote = batchMap.GetValueOrDefault(wt.BatchId, wt.BatchId),
                    Rate = (decimal)workTypeRateMap.GetValueOrDefault(wt.WorkTypeId, 0),
                    Hours = wt.MinutesWorked / 60,
                    Minutes = wt.MinutesWorked % 60,
                    Date = wt.Date
                })
                .ToList();

            EntriesView.ItemsSource = allEntries;
            UpdateStats();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los registros: {ex.Message}", "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("/newtimeentry");
    }

    private async void OnEditEntryTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is TimeEntry entry && !string.IsNullOrEmpty(entry.Id))
        {
            await Shell.Current.GoToAsync("edittimeentry", new Dictionary<string, object>
            {
                { "entryId", entry.Id }
            });
        }
    }

    private void OnNewEntrySaved(WorkedTimeDto dto)
    {
        var entry = new TimeEntry
        {
            Id = dto.Id,
            WorkerId = dto.WorkerId,
            WorkerName = workerMap.GetValueOrDefault(dto.WorkerId, dto.WorkerId),
            WorkerIdentification = workerIdentificationMap.GetValueOrDefault(dto.WorkerId, string.Empty),
            ActivityName = workTypeMap.GetValueOrDefault(dto.WorkTypeId, dto.WorkTypeId),
            Lote = batchMap.GetValueOrDefault(dto.BatchId, dto.BatchId),
            Rate = (decimal)workTypeRateMap.GetValueOrDefault(dto.WorkTypeId, 0),
            Hours = dto.MinutesWorked / 60,
            Minutes = dto.MinutesWorked % 60,
            Date = dto.Date
        };

        allEntries.Insert(0, entry);
        EntriesView.ItemsSource = null;
        EntriesView.ItemsSource = allEntries;
        UpdateStats();
    }

    private void UpdateStats()
    {
        var today = DateTime.Today;
        var todayEntries = allEntries.Where(e => e.Date.Date == today).ToList();

        TodayCount.Text = todayEntries.Count.ToString();

        var todayHours = todayEntries.Sum(e => e.Hours + (e.Minutes / 60m));
        TotalHours.Text = $"{todayHours:F0}h";

        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weekEntries = allEntries.Where(e => e.Date.Date >= weekStart).ToList();
        WeekCount.Text = weekEntries.Count.ToString();
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            EntriesView.ItemsSource = allEntries;
            return;
        }

        var filtered = allEntries
            .Where(entry =>
                entry.WorkerName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                entry.ActivityName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                entry.Lote.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        EntriesView.ItemsSource = filtered;
    }
}
