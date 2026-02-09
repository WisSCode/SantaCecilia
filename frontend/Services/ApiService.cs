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
}
