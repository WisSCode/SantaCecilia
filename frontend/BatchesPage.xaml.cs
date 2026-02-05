using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace frontend;

public partial class BatchesPage : ContentPage
{
    private List<BatchItem> batches = new();

    public BatchesPage()
    {
        InitializeComponent();
        LoadDemoBatches();
    }

    private void LoadDemoBatches()
    {
        batches = new List<BatchItem>
        {
            new BatchItem { Name = "Lote A", Location = "Sector Norte" },
            new BatchItem { Name = "Lote B", Location = "Sector Sur" },
            new BatchItem { Name = "Lote C", Location = "Sector Este" },
            new BatchItem { Name = "Lote D", Location = "Sector Oeste" }
        };

        BatchesList.ItemsSource = batches;
        TotalBatchesLabel.Text = batches.Count.ToString();
        TotalLocationsLabel.Text = batches.Select(b => b.Location).Distinct().Count().ToString();
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Agregar Lote", "Formulario para crear nuevo lote.", "OK");
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is BatchItem batch)
        {
            await DisplayAlertAsync("Editar Lote", $"Editar: {batch.Name}", "OK");
        }
    }
}

public class BatchItem
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
