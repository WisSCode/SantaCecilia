using frontend.Services;
using Microsoft.Maui.Graphics;

namespace frontend.Pages;

[QueryProperty(nameof(UserId), "userId")]
[QueryProperty(nameof(UserEmail), "userEmail")]
public partial class ValidateUserPage : ContentPage
{
    private readonly ApiService _api;
    private string _userId = string.Empty;
    private string _userEmail = string.Empty;
    private List<WorkerDisplayItem> workerItems = [];
    private List<WorkerDisplayItem> filteredWorkerItems = [];
    private WorkerDisplayItem? selectedWorker;

    public string UserId
    {
        get => _userId;
        set => _userId = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public string UserEmail
    {
        get => _userEmail;
        set => _userEmail = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public ValidateUserPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        EmailLabel.Text = _userEmail;
        SubtitleLabel.Text = $"Asigne un trabajador a {_userEmail}";
        selectedWorker = null;
        SelectedWorkerLabel.Text = "Ningún trabajador seleccionado";
        ValidateButton.IsEnabled = false;
        await LoadWorkersAsync();
    }

    private async Task LoadWorkersAsync()
    {
        try
        {
            var allWorkers = await _api.GetWorkersAsync();

            var available = allWorkers
                .Where(w => string.IsNullOrEmpty(w.UserId))
                .ToList();

            if (available.Count == 0)
            {
                NoWorkersLabel.IsVisible = true;
                WorkersListView.IsVisible = false;
                ValidateButton.IsEnabled = false;
                return;
            }

            workerItems = available.Select(w => new WorkerDisplayItem
            {
                Id = w.Id,
                Name = w.Name,
                LastName = w.LastName,
                Identification = w.Identification,
                Active = w.Active,
                OriginalUserId = w.UserId,
                IsSelected = false
            })
            .OrderBy(w => w.Name)
            .ThenBy(w => w.LastName)
            .ToList();

            ApplyWorkerFilter(string.Empty);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los trabajadores: {ex.Message}", "OK");
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        ApplyWorkerFilter(e.NewTextValue ?? string.Empty);
    }

    private void ApplyWorkerFilter(string searchText)
    {
        var query = searchText.Trim();

        filteredWorkerItems = string.IsNullOrWhiteSpace(query)
            ? workerItems.ToList()
            : workerItems.Where(w =>
                w.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                w.Identification.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        WorkersListView.ItemsSource = filteredWorkerItems;

        NoWorkersLabel.IsVisible = filteredWorkerItems.Count == 0;
        WorkersListView.IsVisible = filteredWorkerItems.Count > 0;
        WorkersCountLabel.Text = filteredWorkerItems.Count == 1
            ? "1 trabajador disponible"
            : $"{filteredWorkerItems.Count} trabajadores disponibles";
    }

    private void OnWorkerSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is WorkerDisplayItem item)
        {
            selectedWorker = workerItems.FirstOrDefault(w => w.Id == item.Id) ?? item;

            foreach (var w in workerItems)
                w.IsSelected = w.Id == selectedWorker.Id;

            ApplyWorkerFilter(SearchEntry.Text ?? string.Empty);
            SelectedWorkerLabel.Text = $"Seleccionado: {selectedWorker.FullName}";

            ValidateButton.IsEnabled = true;
        }
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        if (selectedWorker is null)
        {
            await DisplayAlertAsync("Validacion", "Debe seleccionar un trabajador.", "OK");
            return;
        }

        try
        {
            ValidateButton.IsEnabled = false;

            var assignDto = new WorkerDto
            {
                Id = selectedWorker.Id,
                UserId = _userId,
                Name = selectedWorker.Name,
                LastName = selectedWorker.LastName,
                Identification = selectedWorker.Identification,
                Active = selectedWorker.Active
            };
            await _api.UpdateWorkerAsync(selectedWorker.Id, assignDto);

            await _api.ValidateUserAsync(_userId);

            await DisplayAlertAsync("Usuario Validado",
                $"{_userEmail} fue validado y asignado a {selectedWorker.Name} {selectedWorker.LastName}.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ValidateButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo validar: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
