using frontend.Models;
using frontend.Services;

namespace frontend.Pages;

public partial class WorkersPage : ContentPage
{
    private readonly ApiService _api;
    private List<Worker> allWorkers = new();

    public WorkersPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWorkersAsync();
    }

    private async Task LoadWorkersAsync()
    {
        try
        {
            var dtos = await _api.GetWorkersAsync();

            allWorkers = dtos.Select((w, index) => new Worker
            {
                Id = w.Id,
                Name = w.Name,
                LastName = w.LastName,
                Identification = w.Identification,
                UserId = w.UserId,
                Active = w.Active,
                SequentialId = index + 1
            }).ToList();

            WorkersList.ItemsSource = allWorkers;
            UpdateStats();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los trabajadores: {ex.Message}", "OK");
        }
    }

    private void UpdateStats()
    {
        var active = allWorkers.Count(w => w.Active);
        var inactive = allWorkers.Count(w => !w.Active);
        ActiveCount.Text = active.ToString();
        InactiveCount.Text = inactive.ToString();
        TotalCount.Text = allWorkers.Count.ToString();
        WorkersCountLabel.Text = $"{allWorkers.Count} trabajadores encontrados";
    }

    private async void OnAddWorkerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("newworker");
    }

    private async void OnToggleActiveTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Worker worker)
        {
            await ToggleWorkerActive(worker);
        }
    }

    private async Task ToggleWorkerActive(Worker worker)
    {
        try
        {
            worker.Active = !worker.Active;
            await _api.UpdateWorkerAsync(worker.Id, new WorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
                LastName = worker.LastName,
                Identification = worker.Identification,
                UserId = worker.UserId,
                Active = worker.Active
            });
            WorkersList.ItemsSource = null;
            WorkersList.ItemsSource = allWorkers;
            UpdateStats();
        }
        catch (Exception ex)
        {
            worker.Active = !worker.Active;
            await DisplayAlertAsync("Error", $"No se pudo actualizar: {ex.Message}", "OK");
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            WorkersList.ItemsSource = allWorkers;
            WorkersCountLabel.Text = $"{allWorkers.Count} trabajadores encontrados";
            return;
        }

        var filtered = allWorkers
            .Where(w =>
                w.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                w.LastName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                w.Identification.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        WorkersList.ItemsSource = filtered;
        WorkersCountLabel.Text = $"{filtered.Count} trabajadores encontrados";
    }
}
