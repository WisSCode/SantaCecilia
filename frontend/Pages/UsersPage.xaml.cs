using frontend.Services;
using Microsoft.Maui.Graphics;

namespace frontend.Pages;

public partial class UsersPage : ContentPage
{
    private readonly ApiService _api;
    private List<UserItem> users = new();
    private string _sortColumn = "Mail";
    private bool _sortAscending = true;

    public UsersPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var dtos = await _api.GetUsersAsync();
            users = dtos.Select(u => new UserItem
            {
                Id = u.Id,
                Mail = u.Email,
                Role = u.Role,
                Validated = u.Validated
            }).ToList();

            ApplySort();
            TotalUsersLabel.Text = users.Count.ToString();
            ValidatedUsersLabel.Text = users.Count(u => u.Validated).ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los usuarios: {ex.Message}", "OK");
        }
    }

    private void ApplySort()
    {
        IEnumerable<UserItem> sorted = _sortColumn switch
        {
            "Mail" => _sortAscending ? users.OrderBy(u => u.Mail) : users.OrderByDescending(u => u.Mail),
            "Role" => _sortAscending ? users.OrderBy(u => u.Role) : users.OrderByDescending(u => u.Role),
            "Status" => _sortAscending ? users.OrderBy(u => u.Validated) : users.OrderByDescending(u => u.Validated),
            _ => users.AsEnumerable()
        };
        UsersList.ItemsSource = sorted.ToList();
        UpdateHeaders();
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

    private void UpdateHeaders()
    {
        var arrow = _sortAscending ? " \u2191" : " \u2193";
        HeaderEmail.Text = "EMAIL" + (_sortColumn == "Mail" ? arrow : "");
        HeaderRole.Text = "ROL" + (_sortColumn == "Role" ? arrow : "");
        HeaderStatus.Text = "ESTADO" + (_sortColumn == "Status" ? arrow : "");
    }

    private void OnSortByEmail(object sender, TappedEventArgs e) => ToggleSort("Mail");
    private void OnSortByRole(object sender, TappedEventArgs e) => ToggleSort("Role");
    private void OnSortByStatus(object sender, TappedEventArgs e) => ToggleSort("Status");

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is UserItem user)
        {
            try
            {
                var allWorkers = await _api.GetWorkersAsync();
                var unassigned = allWorkers
                    .Where(w => string.IsNullOrEmpty(w.UserId))
                    .ToList();

                if (unassigned.Count == 0)
                {
                    await DisplayAlertAsync("Sin trabajadores", "No hay trabajadores disponibles sin usuario asignado.", "OK");
                    return;
                }

                var options = unassigned
                    .Select(w => $"{w.Name} {w.LastName} - {w.Identification}")
                    .ToArray();

                var selected = await DisplayActionSheetAsync(
                    $"Asignar trabajador a {user.Mail}",
                    "Cancelar",
                    null,
                    options);

                if (selected is null || selected == "Cancelar") return;

                var selectedIndex = Array.IndexOf(options, selected);
                if (selectedIndex < 0) return;

                var worker = unassigned[selectedIndex];

                var workerDto = new WorkerDto
                {
                    Id = worker.Id,
                    UserId = user.Id,
                    Name = worker.Name,
                    LastName = worker.LastName,
                    Identification = worker.Identification,
                    Active = worker.Active
                };
                await _api.UpdateWorkerAsync(worker.Id, workerDto);

                await _api.ValidateUserAsync(user.Id);

                await LoadUsersAsync();

                await DisplayAlertAsync("Usuario Validado",
                    $"{user.Mail} fue validado y asignado a {worker.Name} {worker.LastName}.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo validar: {ex.Message}", "OK");
            }
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is UserItem user)
        {
            var parameters = new Dictionary<string, object>
            {
                { "userId", user.Id },
                { "userEmail", user.Mail }
            };
            await Shell.Current.GoToAsync("/edituser", parameters);
        }
    }
}

public class UserItem
{
    public string Id { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Validated { get; set; }

    public string StatusText => Validated ? "Validado" : "Pendiente";
    public Color StatusColor => Validated ? Color.FromArgb("#9BC7B3") : Color.FromArgb("#F4A261");
    public bool NeedsValidation => !Validated;
}
