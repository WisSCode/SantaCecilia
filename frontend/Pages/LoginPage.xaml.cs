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

        // Login exitoso - cambiar a AppShell con dashboard
        if (Application.Current is App app)
            await app.GoToDashboardAsync();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var registerPage = Handler?.MauiContext?.Services.GetRequiredService<RegisterPage>();
        if (registerPage is not null)
            await Navigation.PushAsync(registerPage);
    }
}