using System.Globalization;
using frontend.Helpers;
using frontend.Services;

namespace frontend.Pages;

public partial class NewWorkTypePage : ContentPage
{
    private readonly ApiService _api;

    public NewWorkTypePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        NameEntry.TextChanged += (s, e) => InputFilter.AllowAlphanumeric((Entry)s!, e);
        RateEntry.TextChanged += (s, e) => InputFilter.AllowDecimalOnly((Entry)s!, e);
    }

    private void OnFieldChanged(object sender, TextChangedEventArgs e)
    {
        var valid = !string.IsNullOrWhiteSpace(NameEntry.Text)
                 && !string.IsNullOrWhiteSpace(RateEntry.Text)
                 && double.TryParse(RateEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate)
                 && rate > 0;
        CreateButton.IsEnabled = valid;
    }

    private async void OnCreateClicked(object sender, EventArgs e)
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
            CreateButton.IsEnabled = false;
            var id = Guid.NewGuid().ToString();
            var dto = new WorkTypeDto
            {
                Id = id,
                Name = name,
                DefaultRate = rate
            };

            await _api.CreateWorkTypeAsync(id, dto);
            await DisplayAlertAsync("Actividad creada", $"La actividad \"{name}\" fue registrada con tarifa B/.{rate:F4}/hr.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            CreateButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo crear la actividad: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
