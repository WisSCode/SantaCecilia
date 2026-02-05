using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using frontend.Data;
using frontend.Services;

namespace frontend;

public partial class ReportsPage : ContentPage
{
    private List<ReportItem> reportItems = new();
    private bool hasLoaded;

    public ReportsPage()
    {
        InitializeComponent();
        LoadDemoReport();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
        var culture = new CultureInfo("es-ES");
        ReportWeekLabel.Text = $"Semana del {weekStart:dd 'de' MMMM 'de' yyyy}";
        if (!hasLoaded)
        {
            await LoadReportAsync(weekStart.Date);
            hasLoaded = true;
        }
    }

    private async Task LoadReportAsync(DateTime weekStart)
    {
        try
        {
            var api = new ApiService(new System.Net.Http.HttpClient());
            var entries = await api.GetEntriesAsync();
            if (entries == null || entries.Count == 0)
            {
                LoadDemoReport();
                return;
            }

            var weekEnd = weekStart.AddDays(7);
            var weekEntries = entries
                .Where(e => e.Date >= weekStart && e.Date < weekEnd)
                .ToList();

            if (weekEntries.Count == 0)
            {
                LoadDemoReport();
                return;
            }

            var minWage = Activities.ActivityList.FirstOrDefault(a => a.Id == "min-wage").Rate;

            reportItems = weekEntries
                .GroupBy(e => e.ActivityId ?? "manual")
                .Select(group =>
                {
                    var activity = Activities.ActivityList.FirstOrDefault(a => a.Id == group.Key);
                    var name = activity.Name ?? group.FirstOrDefault()?.ActivityName ?? group.Key;
                    var category = activity.Category ?? "general";
                    var rate = group.FirstOrDefault()?.Rate ?? 0m;
                    if (rate <= 0)
                    {
                        rate = activity.Rate > 0 ? activity.Rate : minWage;
                    }
                    var totalHours = group.Sum(e => e.Hours + (e.Minutes / 60m));
                    return new ReportItem(name, category, totalHours, rate);
                })
                .OrderByDescending(r => r.Hours)
                .ToList();

            ReportList.ItemsSource = reportItems;
            var totalHours = reportItems.Sum(r => r.Hours);
            var totalAmount = reportItems.Sum(r => r.Amount);
            TotalHoursLabel.Text = $"{totalHours:F1}h";
            TotalAmountLabel.Text = $"B/.{totalAmount:F2}";
        }
        catch
        {
            LoadDemoReport();
        }
    }

    private void LoadDemoReport()
    {
        var items = Activities.ActivityList.Take(6).ToList();
        reportItems = new List<ReportItem>
        {
            new ReportItem(items[0].Name, items[0].Category, 46.5m, items[0].Rate),
            new ReportItem(items[1].Name, items[1].Category, 38.0m, items[1].Rate),
            new ReportItem(items[2].Name, items[2].Category, 52.25m, items[2].Rate),
            new ReportItem(items[3].Name, items[3].Category, 29.0m, items[3].Rate),
            new ReportItem(items[4].Name, items[4].Category, 18.5m, items[4].Rate),
            new ReportItem(items[5].Name, items[5].Category, 41.75m, items[5].Rate),
        };

        ReportList.ItemsSource = reportItems;
        var totalHours = reportItems.Sum(r => r.Hours);
        var totalAmount = reportItems.Sum(r => r.Amount);
        TotalHoursLabel.Text = $"{totalHours:F1}h";
        TotalAmountLabel.Text = $"B/.{totalAmount:F2}";
    }

    private class ReportItem
    {
        public ReportItem(string activityName, string category, decimal hours, decimal rate)
        {
            ActivityName = activityName;
            Category = category;
            Hours = hours;
            Rate = rate;
            Amount = hours * rate;
        }

        public string ActivityName { get; }
        public string Category { get; }
        public decimal Hours { get; }
        public decimal Rate { get; }
        public decimal Amount { get; }
    }
}
