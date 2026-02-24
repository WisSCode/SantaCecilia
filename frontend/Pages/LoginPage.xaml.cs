using frontend.ViewModels;
using frontend.Configuration;

namespace frontend.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;
    private bool _isPasswordVisible;
    private bool _autoLoginAttempted;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        UpdatePasswordVisibilityState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_autoLoginAttempted || !AppSettings.DevAutoLogin.Enabled)
            return;

        _autoLoginAttempted = true;

        _viewModel.Email = AppSettings.DevAutoLogin.Email;
        _viewModel.Password = AppSettings.DevAutoLogin.Password;

        var result = await _viewModel.ExecuteLoginAsync();
        if (result != null && Application.Current is App app)
            await app.GoToShellAsync();
    }

    private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        UpdatePasswordVisibilityState();
    }

    private void UpdatePasswordVisibilityState()
    {
        PasswordEntry.IsPassword = !_isPasswordVisible;
        PasswordToggleButton.Text = _isPasswordVisible ? "Ocultar" : "Mostrar";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var result = await _viewModel.ExecuteLoginAsync();

        if (result == null)
        {
            var errorMsg = string.IsNullOrEmpty(_viewModel.ErrorMessage) 
                ? "Error de autenticación. Verifica tus credenciales." 
                : _viewModel.ErrorMessage;
            
            await DisplayAlertAsync("Error de inicio de sesión", errorMsg, "OK");
            return;
        }

        // Login exitoso - navegar segun rol
        if (Application.Current is App app)
            await app.GoToShellAsync();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var registerPage = Handler?.MauiContext?.Services.GetRequiredService<RegisterPage>();
        if (registerPage is not null)
            await Navigation.PushAsync(registerPage);
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.Email))
        {
            await DisplayAlertAsync(
                "Restablecer contrasena",
                "Ingresa tu correo electronico primero.",
                "OK");
            return;
        }

        bool confirm = await DisplayAlertAsync(
            "Restablecer contrasena",
            $"Enviar enlace de recuperacion a:\n{_viewModel.Email}?",
            "Enviar",
            "Cancelar");

        if (!confirm)
            return;

        try
        {
            await _viewModel.SendPasswordResetAsync();
            await DisplayAlertAsync(
                "Correo enviado",
                "Revisa tu correo para restablecer tu contrasena",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }


}