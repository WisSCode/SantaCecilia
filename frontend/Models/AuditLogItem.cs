using frontend.Services;
using Microsoft.Maui.Graphics;

namespace frontend.Models;

public sealed class AuditLogItem
{
    public AuditLogItem(AuditLogDto dto)
    {
        Action = dto.Action ?? string.Empty;
        Entity = dto.Entity ?? string.Empty;
        EntityId = dto.EntityId ?? string.Empty;
        ActorId = dto.ActorId ?? string.Empty;
        Message = dto.Message ?? string.Empty;
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

    public Color ActionBgColor => Action?.ToLower() switch
    {
        "create" => Color.FromArgb("#E8F5EE"),
        "update" => Color.FromArgb("#FFF4E1"),
        "delete" => Color.FromArgb("#FDECEA"),
        "process" => Color.FromArgb("#E6F0FA"),
        "validate" => Color.FromArgb("#E8F5EE"),
        _ => Color.FromArgb("#F5F0E8")
    };

    public Color ActionTextColor => Action?.ToLower() switch
    {
        "create" => Color.FromArgb("#2E7D5B"),
        "update" => Color.FromArgb("#B87333"),
        "delete" => Color.FromArgb("#C0392B"),
        "process" => Color.FromArgb("#3C6EAA"),
        "validate" => Color.FromArgb("#2E7D5B"),
        _ => Color.FromArgb("#7A6E5D")
    };
}
