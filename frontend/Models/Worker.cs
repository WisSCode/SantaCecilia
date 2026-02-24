namespace frontend.Models;

public class Worker
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public bool Active { get; set; } = true;
    public string DisplayId => Id;
    public string StatusText => Active ? "Activo" : "Inactivo";
    public Microsoft.Maui.Graphics.Color StatusTextColor => Active
        ? Microsoft.Maui.Graphics.Color.FromArgb("#2E7D5B")
        : Microsoft.Maui.Graphics.Color.FromArgb("#7A6E5D");
    public Microsoft.Maui.Graphics.Color StatusBgColor => Active
        ? Microsoft.Maui.Graphics.Color.FromArgb("#E8F5EE")
        : Microsoft.Maui.Graphics.Color.FromArgb("#F5F0E8");
    public string ToggleText => Active ? "Desactivar" : "Activar";
    public Microsoft.Maui.Graphics.Color ToggleColor => Active
        ? Microsoft.Maui.Graphics.Color.FromArgb("#E07A5F")
        : Microsoft.Maui.Graphics.Color.FromArgb("#2E7D5B");
}
