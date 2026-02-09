using frontend.Services;

namespace frontend.Pages;

public partial class BatchesPage : ContentPage
{
    private readonly ApiService _api;
    private List<BatchDto> batches = new();

    public BatchesPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBatchesAsync();
    }

    private async Task LoadBatchesAsync()
    {
        try
        {
            batches = await _api.GetBatchesAsync();
            BatchesList.ItemsSource = batches;
            TotalBatchesLabel.Text = batches.Count.ToString();
            TotalLocationsLabel.Text = batches.Select(b => b.Location).Distinct().Count().ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los lotes: {ex.Message}", "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Agregar Lote", "Formulario para crear nuevo lote.", "OK");
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is BatchDto batch)
        {
            await DisplayAlertAsync("Editar Lote", $"Editar: {batch.Name}", "OK");
        }
    }
}
