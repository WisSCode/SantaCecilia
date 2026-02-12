using frontend.Services;
using Microsoft.Maui.Graphics;

namespace frontend.Pages;

[QueryProperty(nameof(UserId), "userId")]
[QueryProperty(nameof(UserEmail), "userEmail")]
public partial class EditUserPage : ContentPage
{
    private readonly ApiService _api;
    private string _userId = string.Empty;
    private string _userEmail = string.Empty;
    private List<WorkerDisplayItem> workerItems = [];
    private WorkerDisplayItem? selectedWorker;
    private string? currentAssignedWorkerId;

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

    public EditUserPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        EmailLabel.Text = _userEmail;
        SubtitleLabel.Text = $"Asigne un trabajador a {_userEmail}";
        await LoadWorkersAsync();
    }

    private async Task LoadWorkersAsync()
    {
        try
        {
            var allWorkers = await _api.GetWorkersAsync();

            var assignedToThisUser = allWorkers.FirstOrDefault(w => w.UserId == _userId);
            currentAssignedWorkerId = assignedToThisUser?.Id;

            var available = allWorkers
                .Where(w => string.IsNullOrEmpty(w.UserId) || w.UserId == _userId)
                .ToList();

            if (available.Count == 0)
            {
                NoWorkersLabel.IsVisible = true;
                WorkersListView.IsVisible = false;
                SaveButton.IsEnabled = false;
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
                IsSelected = w.UserId == _userId
            }).ToList();

            WorkersListView.ItemsSource = workerItems;

            var currentMatch = workerItems.FirstOrDefault(w => w.IsSelected);
            if (currentMatch != null)
            {
                selectedWorker = currentMatch;
                WorkersListView.SelectedItem = currentMatch;
                SaveButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los trabajadores: {ex.Message}", "OK");
        }
    }

    private void OnWorkerSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is WorkerDisplayItem item)
        {
            selectedWorker = item;

            foreach (var w in workerItems)
                w.IsSelected = w.Id == item.Id;

            WorkersListView.ItemsSource = null;
            WorkersListView.ItemsSource = workerItems;

            SaveButton.IsEnabled = true;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (selectedWorker is null)
        {
            await DisplayAlertAsync("Validacion", "Debe seleccionar un trabajador.", "OK");
            return;
        }

        try
        {
            SaveButton.IsEnabled = false;

            if (currentAssignedWorkerId != null && currentAssignedWorkerId != selectedWorker.Id)
            {
                var oldWorker = workerItems.FirstOrDefault(w => w.Id == currentAssignedWorkerId);
                if (oldWorker != null)
                {
                    var unassignDto = new WorkerDto
                    {
                        Id = oldWorker.Id,
                        UserId = null,
                        Name = oldWorker.Name,
                        LastName = oldWorker.LastName,
                        Identification = oldWorker.Identification,
                        Active = oldWorker.Active
                    };
                    await _api.UpdateWorkerAsync(oldWorker.Id, unassignDto);
                }
            }

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

            await DisplayAlertAsync("Usuario actualizado",
                $"{_userEmail} fue asignado a {selectedWorker.Name} {selectedWorker.LastName}.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            SaveButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo actualizar: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

public class WorkerDisplayItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string? OriginalUserId { get; set; }
    public bool IsSelected { get; set; }

    public string FullName => $"{Name} {LastName}";
    public Color BackgroundColor => IsSelected
        ? Color.FromArgb("#E8F5EE")
        : Colors.White;
}
