namespace frontend.Models;

public class Worker
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string WorkerType { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public bool Active { get; set; } = true;

    public string FullName => $"{Name} {LastName}";
    public string StatusText => Active ? "Activo" : "Inactivo";
    public Microsoft.Maui.Graphics.Color StatusColor => Active 
        ? Microsoft.Maui.Graphics.Color.FromArgb("#9BC7B3") 
        : Microsoft.Maui.Graphics.Color.FromArgb("#E2DDD3");
    public string ToggleText => Active ? "Desactivar" : "Activar";
}
