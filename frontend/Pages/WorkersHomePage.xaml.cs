using System.Globalization;
using frontend.Helpers;
using frontend.Services;

namespace frontend.Pages;

public partial class WorkersHomePage : ContentPage
{
    private readonly SessionService _sessionService;
    private readonly ApiService _api;

    private const double SocialSecurityRate = 0.09;
    private const double EducationInsuranceRate = 0.0125;
    private const double UnionFee = 1.00;

    public WorkersHomePage(SessionService sessionService, ApiService api)
    {
        InitializeComponent();
        _sessionService = sessionService;
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var workers = await _api.GetWorkersAsync();
            var currentWorker = workers.FirstOrDefault(w =>
                !string.IsNullOrEmpty(w.UserId) &&
                w.UserId.Equals(_sessionService.UserId, StringComparison.OrdinalIgnoreCase));

            if (currentWorker == null)
            {
                WorkerNameLabel.Text = "Trabajador no encontrado";
                WorkerCodeLabel.Text = "";
                WorkerCedulaLabel.Text = "";
                AvatarLabel.Text = "?";
                return;
            }

            var fullName = $"{currentWorker.Name} {currentWorker.LastName}";

            // Iniciales: primera letra del nombre + primera letra del apellido
            var initials = "";
            if (currentWorker.Name?.Length > 0)
                initials += currentWorker.Name[0].ToString().ToUpper();
            if (currentWorker.LastName?.Length > 0)
                initials += currentWorker.LastName[0].ToString().ToUpper();
            AvatarLabel.Text = initials.Length > 0 ? initials : "?";

            WorkerNameLabel.Text = fullName;
            WorkerCodeLabel.Text = $"TRB-{currentWorker.Id}";
            WorkerCedulaLabel.Text = $"Cédula: {currentWorker.Identification}";

            var workedTimes = await _api.GetWorkedTimesAsync();
            var workTypes = await _api.GetWorkTypesAsync();
            var batches = await _api.GetBatchesAsync();

            var workTypeMap = workTypes.ToDictionary(wt => wt.Id, wt => wt);
            var batchMap = batches.ToDictionary(b => b.Id, b => b.Name);

            var myWorkedTimes = workedTimes
                .Where(wt => wt.WorkerId == currentWorker.Id)
                .OrderByDescending(wt => wt.Date)
                .ToList();

            var (weekStart, weekEnd) = WeekHelper.GetWeekRange(DateTime.Now);

            var thisWeekEntries = myWorkedTimes
                .Where(wt => wt.Date.Date >= weekStart && wt.Date.Date < weekEnd)
                .ToList();

            double totalMinutes = thisWeekEntries.Sum(wt => wt.MinutesWorked);
            double totalHours = totalMinutes / 60.0;

            double gross = 0;
            foreach (var entry in thisWeekEntries)
            {
                double hours = entry.MinutesWorked / 60.0;
                double rate = workTypeMap.TryGetValue(entry.WorkTypeId, out var wt) ? wt.DefaultRate : 0;
                gross += hours * rate;
            }

            double ssDed = Math.Round(gross * SocialSecurityRate, 2);
            double seDed = Math.Round(gross * EducationInsuranceRate, 2);
            double totalDeductions = ssDed + seDed + UnionFee;
            double net = Math.Round(gross - totalDeductions, 2);

            WeeklyHoursLabel.Text = $"{totalHours:F1}h";
            GrossAmountLabel.Text = $"B/.{gross:F2}";
            SsDeductionLabel.Text = $"-B/.{ssDed:F2}";
            SeDeductionLabel.Text = $"-B/.{seDed:F2}";
            UnionDeductionLabel.Text = $"-B/.{UnionFee:F2}";
            NetPayLabel.Text = $"B/.{net:F2}";
            TotalNetLabel.Text = $"B/.{net:F2}";

            var recentActivities = thisWeekEntries
                .OrderByDescending(wt => wt.Date)
                .Take(10).Select(wt =>
            {
                double hours = wt.MinutesWorked / 60.0;
                var wtType = workTypeMap.TryGetValue(wt.WorkTypeId, out var t) ? t : null;
                double rate = wtType?.DefaultRate ?? 0;
                string batchName = batchMap.TryGetValue(wt.BatchId, out var bn) ? bn : wt.BatchId;

                return new ActivityRow
                {
                    Fecha = wt.Date.ToString("dd MMM", new CultureInfo("es-ES")),
                    Actividad = wtType?.Name ?? wt.WorkTypeId,
                    Lote = batchName,
                    Horas = $"{hours:F1}h",
                    Tarifa = $"B/.{rate:F4}",
                    Monto = $"B/.{hours * rate:F2}"
                };
            }).ToList();

            ActivitiesCollection.ItemsSource = recentActivities;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los datos: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _sessionService.LogoutAsync();
        if (Application.Current is App app)
            await app.GoToLoginAsync();
    }
}

public class ActivityRow
{
    public string Fecha { get; set; } = string.Empty;
    public string Actividad { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public string Horas { get; set; } = string.Empty;
    public string Tarifa { get; set; } = string.Empty;
    public string Monto { get; set; } = string.Empty;
}