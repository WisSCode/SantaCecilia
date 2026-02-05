using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace frontend;

public partial class WorkTypesPage : ContentPage
{
    private List<WorkTypeItem> workTypes = new();

    public WorkTypesPage()
    {
        InitializeComponent();
        LoadDemoWorkTypes();
    }

    private void LoadDemoWorkTypes()
    {
        workTypes = new List<WorkTypeItem>
        {
            new WorkTypeItem { Name = "Control de Sigatoka", DefaultRate = 0.9375 },
            new WorkTypeItem { Name = "Mantenimiento Semillero", DefaultRate = 0.8955 },
            new WorkTypeItem { Name = "Mecánico", DefaultRate = 1.0125 },
            new WorkTypeItem { Name = "Fertilización", DefaultRate = 0.88 },
            new WorkTypeItem { Name = "Cosecha", DefaultRate = 0.95 }
        };

        WorkTypesList.ItemsSource = workTypes;
        TotalTypesLabel.Text = workTypes.Count.ToString();
        AvgRateLabel.Text = $"B/.{workTypes.Average(wt => wt.DefaultRate):F2}";
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Agregar Tipo", "Formulario para crear nueva actividad.", "OK");
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is WorkTypeItem workType)
        {
            await DisplayAlertAsync("Editar Tipo", $"Editar: {workType.Name}", "OK");
        }
    }
}

public class WorkTypeItem
{
    public string Name { get; set; } = string.Empty;
    public double DefaultRate { get; set; }
}
