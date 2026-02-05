using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using frontend.Models;
using frontend.Services;
using frontend.Data;

namespace frontend;

public partial class TimeTrackingPage : ContentPage
{
    private List<TimeEntry> allEntries = new();
    private bool hasLoaded;

    public TimeTrackingPage()
    {
        InitializeComponent();
        LoadDemoData();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        TodayLabel.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM", new System.Globalization.CultureInfo("es-ES"));
        EventBus.NewEntrySaved += OnNewEntrySaved;
        if (!hasLoaded)
        {
            await LoadEntriesAsync();
            hasLoaded = true;
        }
        UpdateStats();
    }

    protected override void OnDisappearing()
    {
        EventBus.NewEntrySaved -= OnNewEntrySaved;
        base.OnDisappearing();
    }

    private void LoadDemoData()
    {
        allEntries = new List<TimeEntry>
        {
            new TimeEntry { WorkerName = "Juan Pérez", ActivityName = "Control de Sigatoka", Lote = "A-12", Hours = 8, Date = DateTime.Now },
            new TimeEntry { WorkerName = "María González", ActivityName = "Mantenimiento semillero", Lote = "B-05", Hours = 7, Date = DateTime.Now },
            new TimeEntry { WorkerName = "Carlos Ramírez", ActivityName = "Mecánico", Lote = "Taller", Hours = 8, Date = DateTime.Now },
        };
        EntriesView.ItemsSource = allEntries;
        UpdateStats();
    }

    private async Task LoadEntriesAsync()
    {
        try
        {
            var api = new ApiService(new System.Net.Http.HttpClient());
            var dtoList = await api.GetEntriesAsync();
            if (dtoList == null || dtoList.Count == 0)
            {
                LoadDemoData();
                return;
            }

            allEntries = dtoList.Select(dto =>
            {
                var activity = Activities.ActivityList.FirstOrDefault(a => a.Id == dto.ActivityId);
                return new TimeEntry
                {
                    WorkerId = dto.WorkerId.ToString(),
                    WorkerName = dto.WorkerName,
                    ActivityId = dto.ActivityId ?? string.Empty,
                    ActivityName = activity.Name ?? dto.ActivityName ?? dto.ActivityId ?? "Actividad",
                    Rate = activity.Rate > 0 ? activity.Rate : dto.Rate,
                    Lote = dto.Lote ?? string.Empty,
                    Date = dto.Date,
                    Hours = dto.Hours,
                    Minutes = dto.Minutes
                };
            }).ToList();

            EntriesView.ItemsSource = allEntries;
        }
        catch
        {
            LoadDemoData();
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        // Aquí se abrirá un modal para agregar nuevo registro
        await Shell.Current.GoToAsync("/newtimeentry");
    }

    private void OnNewEntrySaved(TimeEntryDto dto)
    {
        var entry = new TimeEntry
        {
            WorkerId = dto.WorkerId.ToString(),
            WorkerName = dto.WorkerName,
            ActivityId = dto.ActivityId,
            ActivityName = dto.ActivityName,
            Rate = dto.Rate,
            Lote = dto.Lote,
            Date = dto.Date,
            Hours = dto.Hours,
            Minutes = dto.Minutes
        };

        allEntries.Insert(0, entry);
        EntriesView.ItemsSource = null;
        EntriesView.ItemsSource = allEntries;
        UpdateStats();
    }

    private void UpdateStats()
    {
        TodayCount.Text = allEntries.Count.ToString();
        var totalHours = allEntries.Sum(e => e.Hours + (e.Minutes / 60m));
        TotalHours.Text = $"{totalHours:F1}h";
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
