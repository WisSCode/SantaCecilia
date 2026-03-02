using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using frontend.Models;
using frontend.Services;
using Microsoft.Maui.Controls;

namespace frontend.Pages;

public partial class LogsPage : ContentPage
{
    private readonly ApiService _api;
    private List<AuditLogItem> _logs = new();

    public LogsPage(ApiService api)
    {
        try
        {
            InitializeComponent();
            _api = api;
            System.Diagnostics.Debug.WriteLine("[LogsPage] Constructor completado");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LogsPage] Error en constructor: {ex}");
            throw;
        }
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("[LogsPage] OnAppearing llamado");
            await LoadLogsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LogsPage] Error en OnAppearing: {ex}");
            await DisplayAlertAsync("Error crítico", $"Error al cargar la página: {ex.Message}", "OK");
        }
    }

    private async Task LoadLogsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[LogsPage] Iniciando carga de logs...");
            
            var dtos = await _api.GetAuditLogsAsync();
            System.Diagnostics.Debug.WriteLine($"[LogsPage] Se obtuvieron {dtos.Count} logs del API");
            
            _logs = dtos
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new AuditLogItem(l))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[LogsPage] Se procesaron {_logs.Count} logs");

            // Actualizar UI en el hilo principal
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LogsList.ItemsSource = null; // Limpiar primero
                LogsList.ItemsSource = _logs;
                TotalLogsLabel.Text = _logs.Count.ToString();
                var last24h = _logs.Count(l => l.CreatedAt >= DateTime.Now.AddHours(-24));
                Last24hLabel.Text = last24h.ToString();
            });
            
            System.Diagnostics.Debug.WriteLine("[LogsPage] Carga completada exitosamente");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LogsPage] Error: {ex}");
            await DisplayAlertAsync("Error", $"No se pudieron cargar los logs: {ex.Message}", "OK");
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            LogsList.ItemsSource = _logs;
        }
        else
        {
            var filtered = _logs.Where(l => 
                l.Entity.ToLower().Contains(searchText) || 
                l.Message.ToLower().Contains(searchText) ||
                l.Action.ToLower().Contains(searchText) ||
                l.ActorId.ToLower().Contains(searchText)).ToList();
            LogsList.ItemsSource = filtered;
        }
    }
}
