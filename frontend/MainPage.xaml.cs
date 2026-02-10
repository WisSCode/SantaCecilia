namespace frontend;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new
        {
            Actividades = new[]
            {
                new { Worker="Juan Pérez", Activity="Control de Sigatoka", Lote="A-12", Rate="B/.0.9368", Hours="8.5h", Date="29 Ene 2026" },
                new { Worker="María González", Activity="Semillero", Lote="B-05", Rate="B/.0.8955", Hours="7.0h", Date="29 Ene 2026" }
            }
        };
    }
}
