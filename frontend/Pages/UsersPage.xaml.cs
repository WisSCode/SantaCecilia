using frontend.Services;
using Microsoft.Maui.Graphics;

namespace frontend.Pages;

public partial class UsersPage : ContentPage
{
    private void OnValidateTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label lbl && lbl.BindingContext is UserItem user)
        {
            var parameters = new Dictionary<string, object>
            {
                { "userId", user.Id },
                { "userEmail", user.Mail }
            };
            MainThread.BeginInvokeOnMainThread(async () =>
                await Shell.Current.GoToAsync("/validateuser", parameters));
        }
    }

    // Soporte para Label+TapGestureRecognizer en acciones
    private void OnEditTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label lbl && lbl.BindingContext is UserItem user)
        {
            // Reutiliza la lógica de OnEditClicked
            var parameters = new Dictionary<string, object>
            {
                { "userId", user.Id },
                { "userEmail", user.Mail }
            };
            MainThread.BeginInvokeOnMainThread(async () =>
                await Shell.Current.GoToAsync("/edituser", parameters));
        }
    }

    private void OnDeleteTapped(object sender, TappedEventArgs e)
    {
        // Si tienes lógica de borrado, implementa aquí
        // Por ahora muestra alerta de ejemplo
        if (sender is Label lbl && lbl.BindingContext is UserItem user)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlertAsync("Eliminar", $"Eliminar usuario: {user.Mail}", "OK"));
        }
    }
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
            var parameters = new Dictionary<string, object>
            {
                { "userId", user.Id },
                { "userEmail", user.Mail }
            };
            await Shell.Current.GoToAsync("/validateuser", parameters);
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

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            ApplySort();
        }
        else
        {
            var filtered = users.Where(u => 
                u.Mail.ToLower().Contains(searchText) || 
                u.Role.ToLower().Contains(searchText)).ToList();
            UsersList.ItemsSource = filtered;
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var pendingUsers = users
            .Where(u => u.NeedsValidation)
            .OrderBy(u => u.Mail)
            .ToList();

        if (pendingUsers.Count == 0)
        {
            await DisplayAlertAsync("Agregar", "No hay correos pendientes por validar.", "OK");
            return;
        }

        var options = pendingUsers.Select(u => u.Mail).ToArray();
        var selectedEmail = await DisplayActionSheetAsync(
            "Selecciona el correo a validar",
            "Cancelar",
            null,
            options);

        if (string.IsNullOrWhiteSpace(selectedEmail) || selectedEmail == "Cancelar")
            return;

        var selectedUser = pendingUsers.FirstOrDefault(u => u.Mail == selectedEmail);
        if (selectedUser is null)
            return;

        var parameters = new Dictionary<string, object>
        {
            { "userId", selectedUser.Id },
            { "userEmail", selectedUser.Mail }
        };

        await Shell.Current.GoToAsync("/validateuser", parameters);
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
