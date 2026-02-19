using System.Globalization;
using frontend.Helpers;
using frontend.Services;
using Microsoft.Maui.Graphics;

namespace frontend.Pages;

[QueryProperty(nameof(EntryId), "entryId")]
public partial class EditEntryPage : ContentPage
{
    private readonly ApiService _api;
    private List<WorkTypeDto> allWorkTypes = [];
    private List<ActivityDisplayItem> filteredActivities = [];
    private List<WorkerDto> workerItems = [];
    private List<BatchDto> batchItems = [];
    private ActivityDisplayItem? selectedActivity;
    private bool dataLoaded;
    private string? _entryId;
    private WorkedTimeDto? _currentEntry;

    public string? EntryId
    {
        get => _entryId;
        set
        {
            _entryId = value;
            if (!string.IsNullOrEmpty(_entryId))
            {
                Task.Run(async () => await LoadEntryAsync(_entryId));
            }
        }
    }

    public EditEntryPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        HoursEntry.TextChanged += (s, e) => InputFilter.AllowIntegerOnly((Entry)s!, e);
        MinutesEntry.TextChanged += (s, e) => InputFilter.AllowIntegerOnly((Entry)s!, e);
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

    private async Task LoadEntryAsync(string id)
    {
        try
        {
            var allEntries = await _api.GetWorkedTimesAsync();
            _currentEntry = allEntries.FirstOrDefault(e => e.Id == id);

            if (_currentEntry != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PopulateFormWithEntry();
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo cargar el registro: {ex.Message}", "OK");
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            workerItems = await _api.GetWorkersAsync();
            allWorkTypes = await _api.GetWorkTypesAsync();
            batchItems = await _api.GetBatchesAsync();

            WorkerPicker.ItemsSource = workerItems.Select(w => $"{w.Name} {w.LastName}").ToList();
            LotePicker.ItemsSource = batchItems.Select(b => b.Name).ToList();

            RefreshActivityList(string.Empty);
            
            if (_currentEntry != null)
            {
                PopulateFormWithEntry();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los datos: {ex.Message}", "OK");
        }
    }

    private void PopulateFormWithEntry()
    {
        if (_currentEntry == null) return;

        // Set worker
        var workerIndex = workerItems.FindIndex(w => w.Id == _currentEntry.WorkerId);
        if (workerIndex >= 0)
        {
            WorkerPicker.SelectedIndex = workerIndex;
        }

        // Set activity
        var activity = allWorkTypes.FirstOrDefault(wt => wt.Id == _currentEntry.WorkTypeId);
        if (activity != null)
        {
            selectedActivity = new ActivityDisplayItem
            {
                Id = activity.Id,
                Name = activity.Name,
                DefaultRate = activity.DefaultRate,
                IsSelected = true
            };
            RefreshActivityList(string.Empty);
        }

        // Set batch
        var batchIndex = batchItems.FindIndex(b => b.Id == _currentEntry.BatchId);
        if (batchIndex >= 0)
        {
            LotePicker.SelectedIndex = batchIndex;
        }

        // Set date
        EntryDatePicker.Date = _currentEntry.Date;

        // Set hours and minutes
        int totalMinutes = _currentEntry.MinutesWorked;
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        HoursEntry.Text = hours.ToString();
        MinutesEntry.Text = minutes.ToString();

        UpdateSaveButtonState();
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

    async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (_currentEntry == null || string.IsNullOrEmpty(_entryId))
        {
            await DisplayAlertAsync("Error", "No se puede actualizar sin registro cargado.", "OK");
            return;
        }

        if (WorkerPicker.SelectedIndex < 0 || selectedActivity == null)
        {
            await DisplayAlertAsync("Validacion", "Debe seleccionar trabajador y actividad.", "OK");
            return;
        }

        if (LotePicker.SelectedIndex < 0)
        {
            await DisplayAlertAsync("Validacion", "Debe seleccionar un lote.", "OK");
            return;
        }

        var worker = workerItems[WorkerPicker.SelectedIndex];

        if (!int.TryParse(HoursEntry.Text, out int hours))
            hours = 0;

        if (!int.TryParse(MinutesEntry.Text, out int minutes))
            minutes = 0;

        if (hours < 0 || hours > 24)
        {
            await DisplayAlertAsync("Validacion", "Horas debe estar entre 0 y 24.", "OK");
            return;
        }

        if (minutes < 0 || minutes > 59)
        {
            await DisplayAlertAsync("Validacion", "Minutos debe estar entre 0 y 59.", "OK");
            return;
        }

        if (hours == 0 && minutes == 0)
        {
            await DisplayAlertAsync("Validacion", "Debe registrar al menos 1 minuto de trabajo.", "OK");
            return;
        }

        var batchId = LotePicker.SelectedIndex >= 0 ? batchItems[LotePicker.SelectedIndex].Id : string.Empty;

        var dto = new WorkedTimeDto
        {
            Id = _entryId,
            WorkerId = worker.Id,
            WorkTypeId = selectedActivity.Id,
            BatchId = batchId,
            MinutesWorked = hours * 60 + minutes,
            Date = EntryDatePicker?.Date ?? DateTime.Today
        };

        try
        {
            UpdateButton.IsEnabled = false;
            await _api.UpdateWorkedTimeAsync(_entryId, dto);
            await DisplayAlertAsync("Actualizado", "Registro actualizado correctamente.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            UpdateButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo actualizar: {ex.Message}", "OK");
        }
    }

    async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_currentEntry == null || string.IsNullOrEmpty(_entryId))
        {
            await DisplayAlertAsync("Error", "No se puede eliminar sin registro cargado.", "OK");
            return;
        }

        bool confirm = await DisplayAlertAsync("Confirmar eliminacion", 
            "¿Está seguro que desea eliminar este registro de tiempo?", 
            "Eliminar", "Cancelar");

        if (!confirm) return;

        try
        {
            await _api.DeleteWorkedTimeAsync(_entryId);
            await DisplayAlertAsync("Eliminado", "Registro eliminado correctamente.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo eliminar: {ex.Message}", "OK");
        }
    }

    void OnFieldChanged(object sender, EventArgs e)
    {
        UpdateSaveButtonState();
    }

    void UpdateSaveButtonState()
    {
        int.TryParse(HoursEntry.Text, out int hours);
        int.TryParse(MinutesEntry.Text, out int minutes);
        var ok = WorkerPicker.SelectedIndex >= 0
              && selectedActivity != null
              && LotePicker.SelectedIndex >= 0
              && (hours > 0 || minutes > 0);
        if (UpdateButton != null)
            UpdateButton.IsEnabled = ok;
    }
}
