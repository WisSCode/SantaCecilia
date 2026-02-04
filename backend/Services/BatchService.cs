using Google.Cloud.Firestore;
using backend.Models;
namespace backend.Services;

public class BatchService
{
    private readonly CollectionReference _batches;
    public BatchService(FirestoreDb db)
    {
        _batches = db.Collection("batches");
    }

    // CREATE
    public async Task CreateAsync(string batchId, Batches batch)
    {
        await _batches.Document(batchId).SetAsync(batch);
    }

    // SELECT
    public async Task<Batches?> GetAsync(string batchId)
    {
        var snapshot = await _batches.Document(batchId).GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<Batches>() : null;
    }

    // UPDATE
    public async Task UpdateAsync(string batchId, Batches batch)
    {
        await _batches.Document(batchId).SetAsync(batch, SetOptions.MergeAll);
    }
}