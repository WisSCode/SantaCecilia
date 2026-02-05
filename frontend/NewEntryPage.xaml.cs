using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using frontend.Data;
using frontend.Services;

namespace frontend
{
    public partial class NewEntryPage : ContentPage
    {
        private readonly List<(string Id, string Name, decimal Rate)> activityItems = new();
        private readonly List<(string Id, string Name)> workerItems = new();
        private bool workersLoaded;

        public NewEntryPage()
        {
            InitializeComponent();

            activityItems.AddRange(Activities.ActivityList.Select(a => (a.Id, a.Name, a.Rate)));

            ActivityPicker.ItemsSource = activityItems.Select(a => a.Name).ToList();
            MinutesPicker.ItemsSource = new List<int> { 0, 15, 30, 45 };

            DatePicker.Date = DateTime.Today;
            MinutesPicker.SelectedIndex = 0;
            ActivityPicker.SelectedIndex = activityItems.Count > 0 ? 0 : -1;
            UpdateRateFromActivity();
            UpdateSaveButtonState();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!workersLoaded)
            {
                await LoadWorkersAsync();
                workersLoaded = true;
            }
        }

        private async Task LoadWorkersAsync()
        {
            try
            {
                var api = new ApiService(new System.Net.Http.HttpClient());
                var workers = await api.GetWorkersAsync();
                workerItems.Clear();
                workerItems.AddRange(workers.Select(w => (w.Id.ToString(), w.Name ?? string.Empty)));
            }
            catch
            {
                workerItems.Clear();
                workerItems.AddRange(new List<(string Id, string Name)>
                {
                    ("TRB-001", "Juan Pérez"),
                    ("TRB-002", "María González"),
                    ("TRB-003", "Carlos Ramírez"),
                    ("TRB-004", "Ana Martínez"),
                });
            }

            WorkerPicker.ItemsSource = workerItems.Select(w => w.Name).ToList();
            WorkerPicker.SelectedIndex = workerItems.Count > 0 ? 0 : -1;
            UpdateSaveButtonState();
        }

        async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            if (WorkerPicker.SelectedIndex < 0 || ActivityPicker.SelectedIndex < 0)
            {
                await DisplayAlertAsync("Validación", "Debe seleccionar trabajador y actividad.", "OK");
                return;
            }

            var worker = workerItems[WorkerPicker.SelectedIndex];
            var activity = activityItems[ActivityPicker.SelectedIndex];

            int hours = 0;
            int minutes = 0;
            if (!int.TryParse(HoursEntry.Text, out hours))
            {
                hours = 0;
            }

            if (MinutesPicker.SelectedItem != null)
            {
                int.TryParse(MinutesPicker.SelectedItem.ToString(), out minutes);
            }

            if (hours < 0 || hours > 24)
            {
                await DisplayAlertAsync("Validación", "Horas debe estar entre 0 y 24.", "OK");
                return;
            }

            if (!(minutes == 0 || minutes == 15 || minutes == 30 || minutes == 45))
            {
                await DisplayAlertAsync("Validación", "Minutos inválidos. Usa 0, 15, 30 o 45.", "OK");
                return;
            }

            var rate = activity.Rate;

            var dto = new TimeEntryDto
            {
                WorkerId = 0,
                WorkerName = worker.Name,
                ActivityId = activity.Id,
                ActivityName = activity.Name,
                Rate = rate,
                Lote = LoteEntry.Text ?? string.Empty,
                Date = DatePicker?.Date ?? DateTime.Today,
                Hours = hours,
                Minutes = minutes
            };

            // Try to post to API; if fails, publish via EventBus for UI fallback
            try
            {
                var api = new frontend.Services.ApiService(new System.Net.Http.HttpClient());
                var ok = await api.PostEntryAsync(dto);
                if (!ok)
                {
                    frontend.Services.EventBus.PublishNewEntry(dto);
                }
            }
            catch
            {
                frontend.Services.EventBus.PublishNewEntry(dto);
            }

            await DisplayAlertAsync("Registro", "Registro guardado correctamente.", "OK");
            await Shell.Current.GoToAsync("..");
        }

        void OnFieldChanged(object sender, EventArgs e)
        {
            UpdateSaveButtonState();
        }

        void OnActivityChanged(object sender, EventArgs e)
        {
            UpdateRateFromActivity();
            UpdateSaveButtonState();
        }

        void UpdateSaveButtonState()
        {
            var ok = WorkerPicker.SelectedIndex >= 0 && ActivityPicker.SelectedIndex >= 0;
            if (SaveButton != null)
            {
                SaveButton.IsEnabled = ok;
            }
        }

        void UpdateRateFromActivity()
        {
            if (ActivityPicker.SelectedIndex >= 0 && ActivityPicker.SelectedIndex < activityItems.Count)
            {
                var rate = activityItems[ActivityPicker.SelectedIndex].Rate;
                RateEntry.Text = rate.ToString("0.0000", CultureInfo.InvariantCulture);
            }
            else
            {
                RateEntry.Text = string.Empty;
            }
        }
    }
}
