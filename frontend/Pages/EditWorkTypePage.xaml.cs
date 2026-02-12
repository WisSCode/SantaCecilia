using System.Globalization;
using frontend.Services;

namespace frontend.Pages;

[QueryProperty(nameof(WorkTypeId), "workTypeId")]
[QueryProperty(nameof(WorkTypeName), "workTypeName")]
[QueryProperty(nameof(WorkTypeRate), "workTypeRate")]
public partial class EditWorkTypePage : ContentPage
{
    private readonly ApiService _api;
    private string _workTypeId = string.Empty;
    private string _workTypeName = string.Empty;
    private string _workTypeRate = string.Empty;

    public string WorkTypeId
    {
        get => _workTypeId;
        set => _workTypeId = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public string WorkTypeName
    {
        get => _workTypeName;
        set => _workTypeName = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public string WorkTypeRate
    {
        get => _workTypeRate;
        set => _workTypeRate = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public EditWorkTypePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NameEntry.Text = _workTypeName;
        RateEntry.Text = _workTypeRate;
    }

    private void OnFieldChanged(object sender, TextChangedEventArgs e)
    {
        var valid = !string.IsNullOrWhiteSpace(NameEntry.Text)
                 && !string.IsNullOrWhiteSpace(RateEntry.Text)
                 && double.TryParse(RateEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate)
                 && rate > 0;
        SaveButton.IsEnabled = valid;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? string.Empty;
        var rateText = RateEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlertAsync("Validacion", "El nombre de la actividad es obligatorio.", "OK");
            return;
        }

        if (!double.TryParse(rateText, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate) || rate <= 0)
        {
            await DisplayAlertAsync("Validacion", "Ingrese un pago por hora valido mayor a 0.", "OK");
            return;
        }

        try
        {
            SaveButton.IsEnabled = false;
            var dto = new WorkTypeDto
            {
                Id = _workTypeId,
                Name = name,
                DefaultRate = rate
            };
            await _api.UpdateWorkTypeAsync(_workTypeId, dto);
            await DisplayAlertAsync("Actividad actualizada", $"La actividad \"{name}\" fue actualizada con tarifa B/.{rate:F4}/hr.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            SaveButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo actualizar la actividad: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
