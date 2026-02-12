using System.Net.Http.Json;

namespace frontend.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    // Workers
    public async Task<List<WorkerDto>> GetWorkersAsync()
    {
        var result = await _http.GetFromJsonAsync<List<WorkerDto>>("api/workers");
        return result ?? [];
    }

    public async Task CreateWorkerAsync(string id, WorkerDto dto)
    {
        await _http.PostAsJsonAsync($"api/workers/{id}", dto);
    }

    public async Task UpdateWorkerAsync(string id, WorkerDto dto)
    {
        await _http.PutAsJsonAsync($"api/workers/{id}", dto);
    }

    // Batches
    public async Task<List<BatchDto>> GetBatchesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<BatchDto>>("api/batches");
        return result ?? [];
    }

    public async Task CreateBatchAsync(string id, BatchDto dto)
    {
        await _http.PostAsJsonAsync($"api/batches/{id}", dto);
    }

    public async Task UpdateBatchAsync(string id, BatchDto dto)
    {
        await _http.PutAsJsonAsync($"api/batches/{id}", dto);
    }

    public async Task DeleteBatchAsync(string id)
    {
        await _http.DeleteAsync($"api/batches/{id}");
    }

    // Work Types
    public async Task<List<WorkTypeDto>> GetWorkTypesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<WorkTypeDto>>("api/workTypes");
        return result ?? [];
    }

    public async Task CreateWorkTypeAsync(string id, WorkTypeDto dto)
    {
        await _http.PostAsJsonAsync($"api/workTypes/{id}", dto);
    }

    public async Task UpdateWorkTypeAsync(string id, WorkTypeDto dto)
    {
        await _http.PutAsJsonAsync($"api/workTypes/{id}", dto);
    }

    public async Task DeleteWorkTypeAsync(string id)
    {
        await _http.DeleteAsync($"api/workTypes/{id}");
    }

    // Worked Times
    public async Task<List<WorkedTimeDto>> GetWorkedTimesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<WorkedTimeDto>>("api/workTimes");
        return result ?? [];
    }

    public async Task CreateWorkedTimeAsync(string id, WorkedTimeDto dto)
    {
        await _http.PostAsJsonAsync($"api/workTimes/{id}", dto);
    }

    public async Task UpdateWorkedTimeAsync(string id, WorkedTimeDto dto)
    {
        await _http.PutAsJsonAsync($"api/workTimes/{id}", dto);
    }

    public async Task DeleteWorkedTimeAsync(string id)
    {
        await _http.DeleteAsync($"api/workTimes/{id}");
    }

    // Payrolls
    public async Task<List<PayrollDto>> GetPayrollsAsync()
    {
        var result = await _http.GetFromJsonAsync<List<PayrollDto>>("api/payrolls");
        return result ?? [];
    }

    public async Task CreatePayrollAsync(string id, PayrollDto dto)
    {
        await _http.PostAsJsonAsync($"api/payrolls/{id}", dto);
    }

    public async Task UpdatePayrollAsync(string id, PayrollDto dto)
    {
        await _http.PutAsJsonAsync($"api/payrolls/{id}", dto);
    }

    public async Task<int> ProcessPayrollAsync(DateTime weekStart)
    {
        var payload = new PayrollProcessRequest
        {
            WeekStart = weekStart.Date
        };

        var response = await _http.PostAsJsonAsync("api/payrolls/process", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PayrollProcessResponse>();
        return result?.Count ?? 0;
    }

    // Users
    public async Task<List<UserDto>> GetUsersAsync()
    {
        var result = await _http.GetFromJsonAsync<List<UserDto>>("api/users");
        return result ?? [];
    }

    public async Task ValidateUserAsync(string id)
    {
        await _http.PutAsync($"api/users/{id}/validate", null);
    }

    // Audit Logs
    public async Task<List<AuditLogDto>> GetAuditLogsAsync(int limit = 200)
    {
        var result = await _http.GetFromJsonAsync<List<AuditLogDto>>($"api/auditLogs?limit={limit}");
        return result ?? [];
    }
}

public class PayrollProcessRequest
{
    public DateTime WeekStart { get; set; }
}

public class PayrollProcessResponse
{
    public int Count { get; set; }
}
