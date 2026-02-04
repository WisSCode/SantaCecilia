using Google.Cloud.Firestore;
using backend.Models;
namespace backend.Services;

public class PayrollService
{
    private readonly CollectionReference _payrolls;
    public PayrollService(FirestoreDb db)
    {
        _payrolls = db.Collection("payrolls");
    }

    // CREATE
    public async Task CreateAsync(string payrollId, Payrolls payroll)
    {
        await _payrolls.Document(payrollId).SetAsync(payroll);
    }

    // SELECT
    public async Task<Payrolls?> GetAsync(string payrollId)
    {
        var snapshot = await _payrolls.Document(payrollId).GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<Payrolls>() : null;
    }

    // UPDATE
    public async Task UpdateAsync(string payrollId, Payrolls payroll)
    {
        await _payrolls.Document(payrollId).SetAsync(payroll, SetOptions.MergeAll);
    }
}