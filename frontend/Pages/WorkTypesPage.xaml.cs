using frontend.Services;

namespace frontend.Pages;

public partial class WorkTypesPage : ContentPage
{
    private readonly ApiService _api;
    private List<WorkTypeDto> workTypes = new();

    public WorkTypesPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWorkTypesAsync();
    }

    private async Task LoadWorkTypesAsync()
    {
        try
        {
            workTypes = await _api.GetWorkTypesAsync();
            WorkTypesList.ItemsSource = workTypes;
            TotalTypesLabel.Text = workTypes.Count.ToString();
            if (workTypes.Count > 0)
                AvgRateLabel.Text = $"B/.{workTypes.Average(wt => wt.DefaultRate):F2}";
            else
                AvgRateLabel.Text = "B/.0.00";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los tipos: {ex.Message}", "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Agregar Tipo", "Formulario para crear nueva actividad.", "OK");
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is WorkTypeDto workType)
        {
            await DisplayAlertAsync("Editar Tipo", $"Editar: {workType.Name}", "OK");
        }
    }
}
