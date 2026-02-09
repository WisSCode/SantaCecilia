using frontend.Services;
using Microsoft.Maui.Graphics;

namespace frontend.Pages;

public partial class UsersPage : ContentPage
{
    private readonly ApiService _api;
    private List<UserItem> users = new();

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

            UsersList.ItemsSource = users;
            TotalUsersLabel.Text = users.Count.ToString();
            ValidatedUsersLabel.Text = users.Count(u => u.Validated).ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los usuarios: {ex.Message}", "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Agregar Usuario", "Formulario para crear nuevo usuario.", "OK");
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is UserItem user)
        {
            try
            {
                await _api.ValidateUserAsync(user.Id);
                user.Validated = true;
                UsersList.ItemsSource = null;
                UsersList.ItemsSource = users;
                ValidatedUsersLabel.Text = users.Count(u => u.Validated).ToString();
                await DisplayAlertAsync("Usuario Validado", $"{user.Mail} ha sido validado.", "OK");
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
            await DisplayAlertAsync("Editar Usuario", $"Editar: {user.Mail}", "OK");
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
