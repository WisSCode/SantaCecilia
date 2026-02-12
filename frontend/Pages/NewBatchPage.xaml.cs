using frontend.Services;

namespace frontend.Pages;

public partial class NewBatchPage : ContentPage
{
    private readonly ApiService _api;

    public NewBatchPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private void OnFieldChanged(object sender, TextChangedEventArgs e)
    {
        var valid = !string.IsNullOrWhiteSpace(NameEntry.Text)
                 && !string.IsNullOrWhiteSpace(LocationEntry.Text);
        CreateButton.IsEnabled = valid;
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? string.Empty;
        var location = LocationEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(location))
        {
            await DisplayAlertAsync("Validacion", "Todos los campos son obligatorios.", "OK");
            return;
        }

        try
        {
            CreateButton.IsEnabled = false;
            var id = Guid.NewGuid().ToString();
            var dto = new BatchDto
            {
                Id = id,
                Name = name,
                Location = location
            };

            await _api.CreateBatchAsync(id, dto);
            await DisplayAlertAsync("Lote creado", $"El lote \"{name}\" fue registrado exitosamente.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            CreateButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo crear el lote: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
