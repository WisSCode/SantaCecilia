using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace frontend;

public partial class UsersPage : ContentPage
{
    private List<UserItem> users = new();

    public UsersPage()
    {
        InitializeComponent();
        LoadDemoUsers();
    }

    private void LoadDemoUsers()
    {
        users = new List<UserItem>
        {
            new UserItem { Mail = "admin@santacecilia.com", Role = "Administrador", Validated = true },
            new UserItem { Mail = "supervisor@santacecilia.com", Role = "Supervisor", Validated = true },
            new UserItem { Mail = "trabajador1@santacecilia.com", Role = "Trabajador", Validated = false },
            new UserItem { Mail = "trabajador2@santacecilia.com", Role = "Trabajador", Validated = true }
        };

        UsersList.ItemsSource = users;
        TotalUsersLabel.Text = users.Count.ToString();
        ValidatedUsersLabel.Text = users.Count(u => u.Validated).ToString();
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Agregar Usuario", "Formulario para crear nuevo usuario.", "OK");
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is UserItem user)
        {
            user.Validated = true;
            UsersList.ItemsSource = null;
            UsersList.ItemsSource = users;
            ValidatedUsersLabel.Text = users.Count(u => u.Validated).ToString();
            await DisplayAlertAsync("Usuario Validado", $"{user.Mail} ha sido validado.", "OK");
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
    public string Mail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Validated { get; set; }
    
    public string StatusText => Validated ? "Validado" : "Pendiente";
    public Color StatusColor => Validated ? Color.FromArgb("#9BC7B3") : Color.FromArgb("#F4A261");
    public bool NeedsValidation => !Validated;
}
