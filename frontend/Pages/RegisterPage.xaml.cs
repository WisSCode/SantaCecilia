using frontend.ViewModels;

namespace frontend.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _viewModel;

    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnRegisterCompleted(object sender, EventArgs e)
    {
        try
        {
            var result = await _viewModel.ExecuteRegisterAsync();

            if (result == null)
            {
                if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
                {
                    await DisplayAlert("Error", _viewModel.ErrorMessage, "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo crear la cuenta", "OK");
                }
                return;
            }

            await DisplayAlert(
                "Registro exitoso",
                "Tu cuenta ha sido creada. Un administrador debe validarla antes de que puedas iniciar sesión.",
                "OK"
            );

            // Volver al login
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}