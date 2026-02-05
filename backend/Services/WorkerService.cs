using Google.Cloud.Firestore;
using backend.Models;
namespace backend.Services;

public class WorkerService
{
    private readonly CollectionReference _workers;
    public WorkerService(FirestoreDb db)
    {
        _workers = db.Collection("workers");
    }

    // CREATE
    public async Task CreateAsync(string workerId, Workers worker)
    {
        await _workers.Document(workerId).SetAsync(worker);
    }

    // SELECT
    public async Task<Workers?> GetAsync(string workerId)
    {
        var snapshot = await _workers.Document(workerId).GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<Workers>() : null;
    }

    // SELECT ALL
    public async Task<List<Workers>> GetAllAsync()
    {
        var list = new List<Workers>();
        var snap = await _workers.GetSnapshotAsync();
        foreach (var doc in snap.Documents)
        {
            if (doc.Exists)
            {
                list.Add(doc.ConvertTo<Workers>());
            }
        }
        return list;
    }

    // UPDATE
    public async Task UpdateAsync(string workerId, Workers worker)
    {
        await _workers.Document(workerId).SetAsync(worker, SetOptions.MergeAll);
    }
}