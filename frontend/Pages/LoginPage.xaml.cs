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
                await DisplayAlert("Error", _viewModel.ErrorMessage, "OK");
            }
            else
            {
                await DisplayAlert("Error", "Credenciales inválidas", "OK");
            }
            return;
        }

        await DisplayAlert("Éxito", $"Bienvenido {result.Email}", "OK");

    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("register");
    }
}