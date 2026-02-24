using frontend.Services;

namespace frontend.Pages;

public partial class BatchesPage : ContentPage
{
    private readonly ApiService _api;
    private List<BatchDto> batches = new();
    private string _sortColumn = "Name";
    private bool _sortAscending = true;

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
            ApplySort();
            TotalBatchesLabel.Text = batches.Count.ToString();
            TotalLocationsLabel.Text = batches.Select(b => b.Location).Distinct().Count().ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los lotes: {ex.Message}", "OK");
        }
    }

    private void ApplySort()
    {
        IEnumerable<BatchDto> sorted = _sortColumn switch
        {
            "Name" => _sortAscending ? batches.OrderBy(b => b.Name) : batches.OrderByDescending(b => b.Name),
            "Location" => _sortAscending ? batches.OrderBy(b => b.Location) : batches.OrderByDescending(b => b.Location),
            _ => batches.AsEnumerable()
        };
        BatchesList.ItemsSource = sorted.ToList();
        UpdateHeaders();
    }

    private void ToggleSort(string column)
    {
        if (_sortColumn == column)
            _sortAscending = !_sortAscending;
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
        ApplySort();
    }

    private void UpdateHeaders()
    {
        var arrow = _sortAscending ? " \u2191" : " \u2193";
        HeaderName.Text = "NOMBRE" + (_sortColumn == "Name" ? arrow : "");
        HeaderLocation.Text = "UBICACION" + (_sortColumn == "Location" ? arrow : "");
    }

    private void OnSortByName(object sender, TappedEventArgs e) => ToggleSort("Name");
    private void OnSortByLocation(object sender, TappedEventArgs e) => ToggleSort("Location");

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("/newbatch");
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            ApplySort();
            return;
        }
        var filtered = batches
            .Where(b => b.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        b.Location.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
        BatchesList.ItemsSource = filtered;
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (TryGetBatch(sender, null, out var batch))
        {
            var parameters = new Dictionary<string, object>
            {
                { "batchId", batch.Id },
                { "batchName", batch.Name },
                { "batchLocation", batch.Location }
            };
            await Shell.Current.GoToAsync("/editbatch", parameters);
        }
    }

    private async void OnEditTapped(object sender, TappedEventArgs e)
    {
        if (TryGetBatch(sender, e.Parameter, out var batch))
        {
            var parameters = new Dictionary<string, object>
            {
                { "batchId", batch.Id },
                { "batchName", batch.Name },
                { "batchLocation", batch.Location }
            };
            await Shell.Current.GoToAsync("/editbatch", parameters);
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (TryGetBatch(sender, null, out var batch))
        {
            bool confirm = await DisplayAlertAsync("Confirmar eliminacion",
                $"¿Está seguro que desea eliminar el lote '{batch.Name}'?\n\nEsta acción no se puede deshacer.",
                "Eliminar", "Cancelar");

            if (!confirm) return;

            try
            {
                await _api.DeleteBatchAsync(batch.Id);
                await DisplayAlertAsync("Eliminado", $"El lote '{batch.Name}' fue eliminado exitosamente.", "OK");
                await LoadBatchesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar el lote: {ex.Message}", "OK");
            }
        }
    }

    private async void OnDeleteTapped(object sender, TappedEventArgs e)
    {
        if (TryGetBatch(sender, e.Parameter, out var batch))
        {
            bool confirm = await DisplayAlertAsync("Confirmar eliminacion",
                $"¿Está seguro que desea eliminar el lote '{batch.Name}'?\n\nEsta acción no se puede deshacer.",
                "Eliminar", "Cancelar");

            if (!confirm) return;

            try
            {
                await _api.DeleteBatchAsync(batch.Id);
                await DisplayAlertAsync("Eliminado", $"El lote '{batch.Name}' fue eliminado exitosamente.", "OK");
                await LoadBatchesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar el lote: {ex.Message}", "OK");
            }
        }
    }

    private static bool TryGetBatch(object sender, object? parameter, out BatchDto batch)
    {
        if (sender is Button btn && btn.CommandParameter is BatchDto buttonBatch)
        {
            batch = buttonBatch;
            return true;
        }

        if (sender is TapGestureRecognizer tap && tap.CommandParameter is BatchDto tapBatch)
        {
            batch = tapBatch;
            return true;
        }

        if (parameter is BatchDto parameterBatch)
        {
            batch = parameterBatch;
            return true;
        }

        batch = null!;
        return false;
    }
}
