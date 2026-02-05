using Microsoft.Maui.Controls;
using System;

namespace frontend;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ErrorContainer.IsVisible = false;
        var username = UsernameEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        // Simple local auth to match React app behavior
        if (username == "admin" && password == "admin")
        {
            Application.Current!.MainPage = new AppShell();
            await Shell.Current.GoToAsync("//dashboard");
        }
        else if (password == "1234" && !string.IsNullOrEmpty(username))
        {
            Application.Current!.MainPage = new AppShell();
            await Shell.Current.GoToAsync("//timetracking");
        }
        else
        {
            ErrorLabel.Text = "Usuario o contraseña incorrectos";
            ErrorContainer.IsVisible = true;
        }
    }
}
