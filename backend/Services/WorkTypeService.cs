using Google.Cloud.Firestore;
using backend.Models;
namespace backend.Services;

public class WorkTypeService
{
    private readonly CollectionReference _workTypes;
    public WorkTypeService(FirestoreDb db)
    {
        _workTypes = db.Collection("workTypes");
    }

    // CREATE
    public async Task CreateAsync(string workTypeId, WorkTypes workType)
    {
        await _workTypes.Document(workTypeId).SetAsync(workType);
    }

    // SELECT
    public async Task<WorkTypes?> GetAsync(string workTypeId)
    {
        var snapshot = await _workTypes.Document(workTypeId).GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<WorkTypes>() : null;
    }

    // UPDATE
    public async Task UpdateAsync(string workTypeId, WorkTypes workType)
    {
        await _workTypes.Document(workTypeId).SetAsync(workType, SetOptions.MergeAll);
    }
}