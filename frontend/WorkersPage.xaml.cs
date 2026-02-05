using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using frontend.Models;
using frontend.Services;

namespace frontend;

public partial class WorkersPage : ContentPage
{
    private List<Worker> allWorkers = new();
    private bool hasLoaded;

    public WorkersPage()
    {
        InitializeComponent();
        LoadDemoWorkers();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!hasLoaded)
        {
            await LoadWorkersAsync();
            hasLoaded = true;
        }
    }

    private void LoadDemoWorkers()
    {
        allWorkers = new List<Worker>
        {
            new Worker { Id = "1", Name = "Juan", LastName = "Pérez Gómez", Identification = "8-123-456", WorkerType = "Control de Sigatoka", HourlyRate = 0.9368m, Active = true, UserId = "user1@mail.com" },
            new Worker { Id = "2", Name = "María", LastName = "González Díaz", Identification = "8-234-567", WorkerType = "Mantenimiento Semillero", HourlyRate = 0.8955m, Active = true, UserId = "user2@mail.com" },
            new Worker { Id = "3", Name = "Carlos", LastName = "Ramírez López", Identification = "8-345-678", WorkerType = "Mecánico", HourlyRate = 1.0126m, Active = true, UserId = null },
            new Worker { Id = "4", Name = "Ana", LastName = "Martínez Silva", Identification = "8-456-789", WorkerType = "Mantenimiento Plantillo", HourlyRate = 0.9042m, Active = true, UserId = null },
            new Worker { Id = "5", Name = "Luis", LastName = "Fernández Castro", Identification = "8-567-890", WorkerType = "Sacar Matas", HourlyRate = 0.7935m, Active = true, UserId = null },
            new Worker { Id = "6", Name = "Rosa", LastName = "Torres Méndez", Identification = "8-678-901", WorkerType = "Regar Herbicida", HourlyRate = 0.7790m, Active = false, UserId = null },
        };
        
        WorkersList.ItemsSource = allWorkers;
        UpdateStats();
    }

    private async Task LoadWorkersAsync()
    {
        try
        {
            var api = new ApiService(new System.Net.Http.HttpClient());
            var workers = await api.GetWorkersAsync();
            if (workers == null || workers.Count == 0)
            {
                LoadDemoWorkers();
                return;
            }

            allWorkers = workers.Select(w => new Worker
            {
                Id = w.Id.ToString(),
                Name = w.Name ?? string.Empty,
                LastName = string.Empty,
                WorkerType = "General",
                HourlyRate = 0.0m,
                Active = true
            }).ToList();

            WorkersList.ItemsSource = allWorkers;
            UpdateStats();
        }
        catch
        {
            LoadDemoWorkers();
        }
    }

    private void UpdateStats()
    {
        var active = allWorkers.Count(w => w.Active);
        var inactive = allWorkers.Count(w => !w.Active);
        ActiveCount.Text = active.ToString();
        InactiveCount.Text = inactive.ToString();
        WorkersCountLabel.Text = $"Total: {allWorkers.Count} trabajadores";
    }

    private async void OnAddWorkerClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Agregar Trabajador", "Funcionalidad en desarrollo", "OK");
    }

    private async void OnEditWorkerClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Worker worker)
        {
            await DisplayAlertAsync("Editar Trabajador", $"Editar: {worker.FullName}", "OK");
        }
    }

    private void OnToggleActiveClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Worker worker)
        {
            worker.Active = !worker.Active;
            WorkersList.ItemsSource = null;
            WorkersList.ItemsSource = allWorkers;
            UpdateStats();
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            WorkersList.ItemsSource = allWorkers;
            return;
        }

        var filtered = allWorkers
            .Where(w =>
                w.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                w.WorkerType.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        WorkersList.ItemsSource = filtered;
    }
}
