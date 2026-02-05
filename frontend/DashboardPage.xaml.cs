using Microsoft.Maui.Controls;
using System;
using frontend.Models;

namespace frontend;

public partial class DashboardPage : ContentPage
{
    private List<TimeEntry> recentActivities = new();

    public DashboardPage()
    {
        InitializeComponent();
        LoadDemoData();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DateLabel.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
    }

    private void LoadDemoData()
    {
        // Demo data
        recentActivities = new List<TimeEntry>
        {
            new TimeEntry
            {
                WorkerName = "Juan Pérez",
                ActivityName = "Control de Sigatoka (deshoje)",
                Lote = "Lote A-12",
                Hours = 8,
                Minutes = 30,
                Date = DateTime.Now.AddDays(-2),
                Rate = 0.9368m
            },
            new TimeEntry
            {
                WorkerName = "María González",
                ActivityName = "Mantenimiento de semillero",
                Lote = "Lote B-05",
                Hours = 7,
                Minutes = 0,
                Date = DateTime.Now.AddDays(-2),
                Rate = 0.8955m
            },
            new TimeEntry
            {
                WorkerName = "Carlos Ramírez",
                ActivityName = "Mecánico",
                Lote = "Taller Central",
                Hours = 8,
                Minutes = 0,
                Date = DateTime.Now.AddDays(-2),
                Rate = 1.0126m
            },
            new TimeEntry
            {
                WorkerName = "Ana Martínez",
                ActivityName = "Mantenimiento de plantillo",
                Lote = "Lote C-18",
                Hours = 7,
                Minutes = 30,
                Date = DateTime.Now.AddDays(-3),
                Rate = 0.9042m
            },
            new TimeEntry
            {
                WorkerName = "Luis Fernández",
                ActivityName = "Sacar matas hospederas",
                Lote = "Lote D-02",
                Hours = 8,
                Minutes = 0,
                Date = DateTime.Now.AddDays(-3),
                Rate = 0.7935m
            }
        };

        RecentActivitiesCollection.ItemsSource = recentActivities;
    }
}
