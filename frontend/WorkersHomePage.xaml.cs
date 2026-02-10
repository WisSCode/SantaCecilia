namespace frontend;

public partial class WorkersHomePage : ContentPage
{
    public WorkersHomePage()
    {
        InitializeComponent();

        BindingContext = new
        {
            Actividades = new[]
            {
                new { Fecha="29 ene", Actividad="Control de Sigatoka (deshoje)", Lote="Lote A-12", Horas="8.5h", Tarifa="B/.0.9368", Monto="B/.7.96" },
                new { Fecha="28 ene", Actividad="Control de Sigatoka (deshoje)", Lote="Lote A-15", Horas="8h", Tarifa="B/.0.9368", Monto="B/.7.49" },
                new { Fecha="27 ene", Actividad="Mantenimiento de semillero", Lote="Lote B-05", Horas="7.5h", Tarifa="B/.0.8955", Monto="B/.6.72" },
                new { Fecha="24 ene", Actividad="Control de Sigatoka (deshoje)", Lote="Lote A-12", Horas="8.5h", Tarifa="B/.0.9368", Monto="B/.7.96" },
                new { Fecha="23 ene", Actividad="Control de Sigatoka (deshoje)", Lote="Lote C-08", Horas="8h", Tarifa="B/.0.9368", Monto="B/.7.49" }
            }
        };
    }
}
