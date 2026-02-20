using frontend.Models;
using frontend.Services;

namespace frontend.Pages;

public partial class WorkersPage : ContentPage
{
    private readonly ApiService _api;
    private List<Worker> allWorkers = new();
    private string _sortColumn = "Id";
    private bool _sortAscending = true;

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

            allWorkers = dtos.Select(w => new Worker
            {
                Id = w.Id,
                Name = w.Name,
                LastName = w.LastName,
                Identification = w.Identification,
                UserId = w.UserId,
                Active = w.Active
            }).ToList();

            ApplySort();
            UpdateStats();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los trabajadores: {ex.Message}", "OK");
        }
    }

    private void ApplySort()
    {
        IEnumerable<Worker> sorted = _sortColumn switch
        {
            "Id" => _sortAscending ? allWorkers.OrderBy(w => w.Id) : allWorkers.OrderByDescending(w => w.Id),
            "Name" => _sortAscending ? allWorkers.OrderBy(w => w.Name) : allWorkers.OrderByDescending(w => w.Name),
            "LastName" => _sortAscending ? allWorkers.OrderBy(w => w.LastName) : allWorkers.OrderByDescending(w => w.LastName),
            "Identification" => _sortAscending ? allWorkers.OrderBy(w => w.Identification) : allWorkers.OrderByDescending(w => w.Identification),
            "Status" => _sortAscending ? allWorkers.OrderBy(w => w.Active) : allWorkers.OrderByDescending(w => w.Active),
            _ => allWorkers.AsEnumerable()
        };
        WorkersList.ItemsSource = sorted.ToList();
        UpdateSortHeaders();
    }

    private void ToggleSort(string column)
    {
        if (_sortColumn == column)
            _sortAscending = !_sortAscending;
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
        ApplySort();
    }

    private void UpdateSortHeaders()
    {
        var arrow = _sortAscending ? " \u2191" : " \u2193";
        HeaderId.Text = "ID" + (_sortColumn == "Id" ? arrow : "");
        HeaderName.Text = "NOMBRE" + (_sortColumn == "Name" ? arrow : "");
        HeaderLastName.Text = "APELLIDO" + (_sortColumn == "LastName" ? arrow : "");
        HeaderId2.Text = "CÉDULA" + (_sortColumn == "Identification" ? arrow : "");
        HeaderStatus.Text = "ESTADO" + (_sortColumn == "Status" ? arrow : "");
    }

    private void OnSortById(object sender, TappedEventArgs e) => ToggleSort("Id");
    private void OnSortByName(object sender, TappedEventArgs e) => ToggleSort("Name");
    private void OnSortByLastName(object sender, TappedEventArgs e) => ToggleSort("LastName");
    private void OnSortByIdentification(object sender, TappedEventArgs e) => ToggleSort("Identification");
    private void OnSortByStatus(object sender, TappedEventArgs e) => ToggleSort("Status");

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

    private async void OnEditWorkerTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Worker worker)
        {
            await Shell.Current.GoToAsync("editworker", new Dictionary<string, object>
            {
                { "worker", worker }
            });
        }
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
            ApplySort();
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
            ApplySort();
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
