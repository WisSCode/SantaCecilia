using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace frontend.Services;

public class ApiService
{
    private readonly HttpClient _http;
    public string BaseUrl { get; set; } = "http://localhost:5191"; // update if backend runs elsewhere

    public ApiService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<List<WorkerDto>> GetWorkersAsync()
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<List<WorkerDto>>("/api/workers");
            if (resp != null && resp.Count > 0) return resp;
            // fallback to sample data when backend returns empty
            return GetSampleWorkers();
        }
        catch
        {
            return GetSampleWorkers();
        }
    }

    public async Task<List<TimeEntryDto>> GetEntriesAsync()
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<List<BackendWorkedTimeDto>>("/api/workTimes");
            var list = new List<TimeEntryDto>();
            if (resp == null) return list;
            foreach (var r in resp)
            {
                var hours = r.MinutesWorked / 60;
                var minutes = r.MinutesWorked % 60;
                list.Add(new TimeEntryDto
                {
                    Id = 0,
                    WorkerId = int.TryParse(r.WorkerId, out var wid) ? wid : 0,
                    WorkerName = r.WorkerId,
                    ActivityId = r.WorkTypeId,
                    ActivityName = r.WorkTypeId,
                    Rate = 0,
                    Lote = r.BatchId,
                    Date = r.date,
                    Hours = hours,
                    Minutes = minutes
                });
            }
            return list;
        }
        catch
        {
            return GetSampleEntries();
        }
    }

    public async Task<bool> PostEntryAsync(TimeEntryDto entry)
    {
        try
        {
            var id = System.Guid.NewGuid().ToString();
            // map TimeEntryDto to backend shape expected by /api/workTimes/{id}
            var payload = new {
                WorkerId = entry.WorkerId.ToString(),
                WorkTypeId = entry.ActivityId ?? "manual",
                BatchId = entry.Lote ?? string.Empty,
                MinutesWorked = entry.Hours * 60 + entry.Minutes,
                date = entry.Date
            };
            var resp = await _http.PostAsJsonAsync($"/api/workTimes/{id}", payload);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // --- Sample data fallbacks for demo without backend ---
    private List<WorkerDto> GetSampleWorkers()
    {
        return new List<WorkerDto>
        {
            new WorkerDto { Id = 1, Name = "Juan Pérez" },
            new WorkerDto { Id = 2, Name = "María González" },
            new WorkerDto { Id = 3, Name = "Carlos Ramírez" }
        };
    }

    private List<TimeEntryDto> GetSampleEntries()
    {
        return new List<TimeEntryDto>
        {
            new TimeEntryDto { Id = 1, WorkerId = 1, WorkerName = "Juan Pérez", ActivityId = "control-sigatoka", ActivityName = "Control de Sigatoka (deshoje)", Rate = 0.9368m, Lote = "Lote A-12", Date = DateTime.Parse("2026-01-29"), Hours = 8, Minutes = 30 },
            new TimeEntryDto { Id = 2, WorkerId = 2, WorkerName = "María González", ActivityId = "mantenimiento-semillero", ActivityName = "Mantenimiento de semillero", Rate = 0.8955m, Lote = "Lote B-05", Date = DateTime.Parse("2026-01-29"), Hours = 7, Minutes = 0 },
            new TimeEntryDto { Id = 3, WorkerId = 3, WorkerName = "Carlos Ramírez", ActivityId = "mecanico", ActivityName = "Mecánico", Rate = 1.0126m, Lote = "Taller Central", Date = DateTime.Parse("2026-01-29"), Hours = 8, Minutes = 0 }
        };
    }

    // Internal type to map backend worked time DTOs when calling real backend
    private class BackendWorkedTimeDto
    {
        public string WorkerId { get; set; } = string.Empty;
        public string WorkTypeId { get; set; } = string.Empty;
        public string BatchId { get; set; } = string.Empty;
        public int MinutesWorked { get; set; }
        public DateTime date { get; set; }
    }
}
