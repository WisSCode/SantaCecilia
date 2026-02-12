using frontend.ViewModels;

namespace frontend.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var result = await _viewModel.ExecuteLoginAsync();

        if (result == null)
        {
            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                await DisplayAlertAsync("Error", _viewModel.ErrorMessage, "OK");
            }
            else
            {
                await DisplayAlertAsync("Error", "Credenciales invalidas", "OK");
            }
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