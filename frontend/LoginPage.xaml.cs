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
            // Tu lógica actual de login
            await DisplayAlert("Login", "Botón Ingresar presionado", "OK");
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("register");
        }
    }
}