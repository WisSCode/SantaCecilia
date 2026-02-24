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
        await FirestoreOperationHelper.ExecuteAsync(() => _workTypes.Document(workTypeId).SetAsync(workType));
    }

    // SELECT
    public async Task<WorkTypes?> GetAsync(string workTypeId)
    {
        var snapshot = await FirestoreOperationHelper.ExecuteAsync(() => _workTypes.Document(workTypeId).GetSnapshotAsync());
        return snapshot.Exists ? snapshot.ConvertTo<WorkTypes>() : null;
    }

    // SELECT ALL
    public async Task<List<(string Id, WorkTypes WorkType)>> GetAllAsync()
    {
        var list = new List<(string, WorkTypes)>();
        var snap = await FirestoreOperationHelper.ExecuteAsync(() => _workTypes.GetSnapshotAsync());
        foreach (var doc in snap.Documents)
        {
            if (doc.Exists)
            {
                list.Add((doc.Id, doc.ConvertTo<WorkTypes>()));
            }
        }
        return list;
    }

    // UPDATE
    public async Task UpdateAsync(string workTypeId, WorkTypes workType)
    {
        await FirestoreOperationHelper.ExecuteAsync(() => _workTypes.Document(workTypeId).SetAsync(workType, SetOptions.MergeAll));
    }

    // DELETE
    public async Task DeleteAsync(string workTypeId)
    {
        await FirestoreOperationHelper.ExecuteAsync(() => _workTypes.Document(workTypeId).DeleteAsync());
    }
}