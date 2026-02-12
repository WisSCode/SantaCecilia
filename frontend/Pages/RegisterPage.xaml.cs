using frontend.ViewModels;

namespace frontend.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _viewModel;
    private bool _isPasswordVisible;

    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        UpdatePasswordVisibilityState();
    }

    private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        UpdatePasswordVisibilityState();
    }

    private void UpdatePasswordVisibilityState()
    {
        RegisterPasswordEntry.IsPassword = !_isPasswordVisible;
        RegisterPasswordToggleButton.Text = _isPasswordVisible ? "Ocultar" : "Mostrar";
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
                    await DisplayAlertAsync("Error", _viewModel.ErrorMessage, "OK");
                }
                else
                {
                    await DisplayAlertAsync("Error", "No se pudo crear la cuenta", "OK");
                }
                return;
            }

            await DisplayAlertAsync(
                "Registro exitoso",
                "Tu cuenta ha sido creada. Un administrador debe validarla antes de que puedas iniciar sesion.",
                "OK"
            );

            // Volver al login
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}