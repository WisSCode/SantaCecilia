using frontend.Services;

namespace frontend.Pages;

public partial class NewWorkerPage : ContentPage
{
    private readonly ApiService _api;

    public NewWorkerPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private void OnFieldChanged(object sender, TextChangedEventArgs e)
    {
        var valid = !string.IsNullOrWhiteSpace(NameEntry.Text)
                 && !string.IsNullOrWhiteSpace(LastNameEntry.Text)
                 && !string.IsNullOrWhiteSpace(IdentificationEntry.Text);
        CreateButton.IsEnabled = valid;
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? string.Empty;
        var lastName = LastNameEntry.Text?.Trim() ?? string.Empty;
        var identification = IdentificationEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(identification))
        {
            await DisplayAlertAsync("Validacion", "Todos los campos son obligatorios.", "OK");
            return;
        }

        try
        {
            CreateButton.IsEnabled = false;
            var id = Guid.NewGuid().ToString();
            var dto = new WorkerDto
            {
                Id = id,
                Name = name,
                LastName = lastName,
                Identification = identification,
                Active = true,
                UserId = null
            };

            await _api.CreateWorkerAsync(id, dto);
            await DisplayAlertAsync("Trabajador creado", $"{name} {lastName} fue registrado exitosamente.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            CreateButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo crear el trabajador: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
