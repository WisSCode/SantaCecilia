using frontend.Helpers;
using frontend.Services;

namespace frontend.Pages;

[QueryProperty(nameof(BatchId), "batchId")]
[QueryProperty(nameof(BatchName), "batchName")]
[QueryProperty(nameof(BatchLocation), "batchLocation")]
public partial class EditBatchPage : ContentPage
{
    private readonly ApiService _api;
    private string _batchId = string.Empty;
    private string _batchName = string.Empty;
    private string _batchLocation = string.Empty;

    public string BatchId
    {
        get => _batchId;
        set => _batchId = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public string BatchName
    {
        get => _batchName;
        set => _batchName = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public string BatchLocation
    {
        get => _batchLocation;
        set => _batchLocation = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public EditBatchPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        NameEntry.TextChanged += (s, e) => InputFilter.AllowAlphanumeric((Entry)s!, e);
        LocationEntry.TextChanged += (s, e) => InputFilter.AllowAlphanumeric((Entry)s!, e);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NameEntry.Text = _batchName;
        LocationEntry.Text = _batchLocation;
    }

    private void OnFieldChanged(object sender, TextChangedEventArgs e)
    {
        var valid = !string.IsNullOrWhiteSpace(NameEntry.Text)
                 && !string.IsNullOrWhiteSpace(LocationEntry.Text);
        SaveButton.IsEnabled = valid;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
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
            SaveButton.IsEnabled = false;
            var dto = new BatchDto
            {
                Id = _batchId,
                Name = name,
                Location = location
            };
            await _api.UpdateBatchAsync(_batchId, dto);
            await DisplayAlertAsync("Lote actualizado", $"El lote \"{name}\" fue actualizado exitosamente.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            SaveButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo actualizar el lote: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
