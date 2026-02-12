using System.Globalization;
using frontend.Services;
using Microsoft.Maui.Graphics;

namespace frontend.Pages;

public partial class NewEntryPage : ContentPage
{
    private readonly ApiService _api;
    private List<WorkTypeDto> allWorkTypes = [];
    private List<ActivityDisplayItem> filteredActivities = [];
    private List<WorkerDto> workerItems = [];
    private List<BatchDto> batchItems = [];
    private ActivityDisplayItem? selectedActivity;
    private bool dataLoaded;

    public NewEntryPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        MinutesPicker.ItemsSource = new List<int> { 0, 15, 30, 45 };
        EntryDatePicker.Date = DateTime.Today;
        MinutesPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!dataLoaded)
        {
            await LoadDataAsync();
            dataLoaded = true;
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            workerItems = await _api.GetWorkersAsync();
            allWorkTypes = await _api.GetWorkTypesAsync();
            batchItems = await _api.GetBatchesAsync();

            WorkerPicker.ItemsSource = workerItems
                .Select(w => string.IsNullOrWhiteSpace(w.Identification)
                    ? $"{w.Name} {w.LastName}"
                    : $"{w.Name} {w.LastName} -> {w.Identification}")
                .ToList();
            WorkerPicker.SelectedIndex = -1;

            LotePicker.ItemsSource = batchItems.Select(b => b.Name).ToList();
            LotePicker.SelectedIndex = -1;

            RefreshActivityList(string.Empty);
            UpdateSaveButtonState();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los datos: {ex.Message}", "OK");
        }
    }

    private void RefreshActivityList(string query)
    {
        var source = allWorkTypes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            source = source.Where(a =>
                a.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        filteredActivities = source.Select(a => new ActivityDisplayItem
        {
            Id = a.Id,
            Name = a.Name,
            DefaultRate = a.DefaultRate,
            IsSelected = selectedActivity != null && selectedActivity.Id == a.Id
        }).ToList();

        ActivityListView.ItemsSource = filteredActivities;

        if (selectedActivity != null)
        {
            var match = filteredActivities.FirstOrDefault(a => a.Id == selectedActivity.Id);
            if (match != null)
                ActivityListView.SelectedItem = match;
        }
    }

    private void OnActivitySearchChanged(object sender, TextChangedEventArgs e)
    {
        RefreshActivityList(e.NewTextValue ?? string.Empty);
    }

    private void OnActivitySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ActivityDisplayItem item)
        {
            selectedActivity = item;

            foreach (var a in filteredActivities)
                a.IsSelected = a.Id == item.Id;

            ActivityListView.ItemsSource = null;
            ActivityListView.ItemsSource = filteredActivities;

            UpdateSaveButtonState();
        }
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        if (WorkerPicker.SelectedIndex < 0 || selectedActivity == null)
        {
            await DisplayAlertAsync("Validacion", "Debe seleccionar trabajador y actividad.", "OK");
            return;
        }

        var worker = workerItems[WorkerPicker.SelectedIndex];

        int hours = 0;
        int minutes = 0;
        if (!int.TryParse(HoursEntry.Text, out hours))
            hours = 0;

        if (MinutesPicker.SelectedItem != null)
            int.TryParse(MinutesPicker.SelectedItem.ToString(), out minutes);

        if (hours < 0 || hours > 24)
        {
            await DisplayAlertAsync("Validacion", "Horas debe estar entre 0 y 24.", "OK");
            return;
        }

        var batchId = LotePicker.SelectedIndex >= 0 ? batchItems[LotePicker.SelectedIndex].Id : string.Empty;

        var dto = new WorkedTimeDto
        {
            WorkerId = worker.Id,
            WorkTypeId = selectedActivity.Id,
            BatchId = batchId,
            MinutesWorked = hours * 60 + minutes,
            Date = EntryDatePicker?.Date ?? DateTime.Today
        };

        try
        {
            SaveButton.IsEnabled = false;
            var id = Guid.NewGuid().ToString();
            await _api.CreateWorkedTimeAsync(id, dto);
            EventBus.PublishNewEntry(dto);
            await DisplayAlertAsync("Registro", "Registro guardado correctamente.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            SaveButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
    }

    void OnFieldChanged(object sender, EventArgs e)
    {
        UpdateSaveButtonState();
    }

    void UpdateSaveButtonState()
    {
        var ok = WorkerPicker.SelectedIndex >= 0
              && selectedActivity != null
              && LotePicker.SelectedIndex >= 0;
        if (SaveButton != null)
            SaveButton.IsEnabled = ok;
    }
}

public class ActivityDisplayItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double DefaultRate { get; set; }
    public bool IsSelected { get; set; }

    public string RateDisplay => $"B/.{DefaultRate:F4}";
    public Color BackgroundColor => IsSelected
        ? Color.FromArgb("#E8F5EE")
        : Colors.White;
}
