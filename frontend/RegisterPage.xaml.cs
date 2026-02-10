namespace frontend;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterCompleted(object sender, EventArgs e)
    {
        // Aquí luego irá la lógica real de registro
        await DisplayAlert("Error", "Campos vacíos", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
