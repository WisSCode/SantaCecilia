using System.Globalization;
using frontend.Services;

namespace frontend.Pages;

public partial class WorkTypesPage : ContentPage
{
    private readonly ApiService _api;
    private List<WorkTypeDto> workTypes = new();
    private string _sortColumn = "Name";
    private bool _sortAscending = true;

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
            ApplySort();
            TotalTypesLabel.Text = workTypes.Count.ToString();
            if (workTypes.Count > 0)
                AvgRateLabel.Text = $"B/.{workTypes.Average(wt => wt.DefaultRate):F4}";
            else
                AvgRateLabel.Text = "B/.0.0000";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los tipos: {ex.Message}", "OK");
        }
    }

    private void ApplySort()
    {
        IEnumerable<WorkTypeDto> sorted = _sortColumn switch
        {
            "Name" => _sortAscending ? workTypes.OrderBy(w => w.Name) : workTypes.OrderByDescending(w => w.Name),
            "Rate" => _sortAscending ? workTypes.OrderBy(w => w.DefaultRate) : workTypes.OrderByDescending(w => w.DefaultRate),
            _ => workTypes.AsEnumerable()
        };
        WorkTypesList.ItemsSource = sorted.ToList();
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
        HeaderRate.Text = "TARIFA" + (_sortColumn == "Rate" ? arrow : "");
    }

    private void OnSortByName(object sender, TappedEventArgs e) => ToggleSort("Name");
    private void OnSortByRate(object sender, TappedEventArgs e) => ToggleSort("Rate");

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("/newworktype");
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            ApplySort();
            return;
        }
        WorkTypesList.ItemsSource = workTypes
            .Where(wt => wt.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is WorkTypeDto workType)
        {
            var parameters = new Dictionary<string, object>
            {
                { "workTypeId", workType.Id },
                { "workTypeName", workType.Name },
                { "workTypeRate", workType.DefaultRate.ToString("F4", CultureInfo.InvariantCulture) }
            };
            await Shell.Current.GoToAsync("/editworktype", parameters);
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is WorkTypeDto workType)
        {
            bool confirm = await DisplayAlertAsync("Confirmar eliminacion",
                $"¿Está seguro que desea eliminar el tipo de trabajo '{workType.Name}'?\n\nEsta acción no se puede deshacer.",
                "Eliminar", "Cancelar");

            if (!confirm) return;

            try
            {
                await _api.DeleteWorkTypeAsync(workType.Id);
                await DisplayAlertAsync("Eliminado", $"El tipo de trabajo '{workType.Name}' fue eliminado exitosamente.", "OK");
                await LoadWorkTypesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar el tipo de trabajo: {ex.Message}", "OK");
            }
        }
    }
}
