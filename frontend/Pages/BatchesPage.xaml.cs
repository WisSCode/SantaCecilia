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
        if (sender is Button btn && btn.CommandParameter is BatchDto batch)
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
}
