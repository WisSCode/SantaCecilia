namespace frontend
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Validación mínima
            if (string.IsNullOrWhiteSpace(UsernameEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Error", "Campos vacíos", "OK");
                return;
            }

            // AQUÍ SIMULAMOS LOGIN CORRECTO
            // Luego aquí irá tu API / backend
            await Shell.Current.GoToAsync("//workersHome");
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("register");
        }
    }
}
