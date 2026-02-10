using System;
using System.Linq;
using System.Threading.Tasks;
using frontend.Services;
using Microsoft.Maui.Graphics;

namespace frontend.Pages;

public partial class LogsPage : ContentPage
{
    private readonly ApiService _api;
    private List<AuditLogItem> _logs = new();

    public LogsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLogsAsync();
    }

    private async Task LoadLogsAsync()
    {
        try
        {
            var dtos = await _api.GetAuditLogsAsync();
            _logs = dtos
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new AuditLogItem(l))
                .ToList();

            LogsList.ItemsSource = _logs;
            TotalLogsLabel.Text = _logs.Count.ToString();
            var last24h = _logs.Count(l => l.CreatedAt >= DateTime.Now.AddHours(-24));
            Last24hLabel.Text = last24h.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los logs: {ex.Message}", "OK");
        }
    }

    private sealed class AuditLogItem
    {
        public AuditLogItem(AuditLogDto dto)
        {
            Action = dto.Action;
            Entity = dto.Entity;
            EntityId = dto.EntityId;
            ActorId = dto.ActorId;
            Message = dto.Message;
            CreatedAt = dto.CreatedAt;
        }

        public string Action { get; }
        public string Entity { get; }
        public string EntityId { get; }
        public string ActorId { get; }
        public string Message { get; }
        public DateTime CreatedAt { get; }

        public string DateDisplay => CreatedAt.ToString("dd/MM/yyyy HH:mm");
        public string ActionText => Action.ToUpperInvariant();

        public Color ActionBgColor => Action switch
        {
            "create" => Color.FromArgb("#E8F5EE"),
            "update" => Color.FromArgb("#FFF4E1"),
            "delete" => Color.FromArgb("#FDECEA"),
            "process" => Color.FromArgb("#E6F0FA"),
            "validate" => Color.FromArgb("#E8F5EE"),
            _ => Color.FromArgb("#F5F0E8")
        };

        public Color ActionTextColor => Action switch
        {
            "create" => Color.FromArgb("#2E7D5B"),
            "update" => Color.FromArgb("#B87333"),
            "delete" => Color.FromArgb("#C0392B"),
            "process" => Color.FromArgb("#3C6EAA"),
            "validate" => Color.FromArgb("#2E7D5B"),
            _ => Color.FromArgb("#7A6E5D")
        };
    }
}
