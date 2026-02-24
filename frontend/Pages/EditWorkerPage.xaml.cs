using frontend.Helpers;
using frontend.Services;
using frontend.Models;

namespace frontend.Pages;

[QueryProperty(nameof(Worker), "worker")]
public partial class EditWorkerPage : ContentPage
{
    private readonly ApiService _api;
    private Worker? _worker;

    public Worker? Worker
    {
        get => _worker;
        set
        {
            _worker = value;
            if (_worker != null)
            {
                LoadWorkerData();
            }
        }
    }

    public EditWorkerPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        NameEntry.TextChanged += (s, e) => InputFilter.AllowLettersOnly((Entry)s!, e);
        LastNameEntry.TextChanged += (s, e) => InputFilter.AllowLettersOnly((Entry)s!, e);
        IdentificationEntry.TextChanged += (s, e) => InputFilter.AllowCedulaFormat((Entry)s!, e);
    }

    private void LoadWorkerData()
    {
        if (_worker == null) return;

        // Cargar datos actuales del trabajador
        NameEntry.Text = _worker.Name;
        LastNameEntry.Text = _worker.LastName;
        IdentificationEntry.Text = _worker.Identification;

        // Habilitar botón inicialmente si todos los campos tienen datos
        ValidateFields();
    }

    private void OnFieldChanged(object sender, TextChangedEventArgs e)
    {
        ValidateFields();
    }

    private void ValidateFields()
    {
        var valid = !string.IsNullOrWhiteSpace(NameEntry.Text)
                 && !string.IsNullOrWhiteSpace(LastNameEntry.Text)
                 && !string.IsNullOrWhiteSpace(IdentificationEntry.Text);
        UpdateButton.IsEnabled = valid;
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (_worker == null) return;

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
            UpdateButton.IsEnabled = false;
            
            var dto = new WorkerDto
            {
                Id = _worker.Id,
                Name = name,
                LastName = lastName,
                Identification = identification,
                Active = _worker.Active,
                UserId = _worker.UserId
            };

            await _api.UpdateWorkerAsync(_worker.Id, dto);
            await DisplayAlertAsync("Trabajador actualizado", $"{name} {lastName} fue actualizado exitosamente.", "OK");
            
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            UpdateButton.IsEnabled = true;
            await DisplayAlertAsync("Error", $"No se pudo actualizar el trabajador: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
