using Google.Cloud.Firestore;
using backend.Models;
namespace backend.Services;

public class WorkedTimeService
{
    private readonly CollectionReference _workedTimes;
    public WorkedTimeService(FirestoreDb db)
    {
        _workedTimes = db.Collection("workedTimes");
    }

    // CREATE
    public async Task CreateAsync(string workedTimeId, WorkedTimes workedTimes)
    {
        await FirestoreOperationHelper.ExecuteAsync(() => _workedTimes.Document(workedTimeId).SetAsync(workedTimes));
    }

    // SELECT
    public async Task<WorkedTimes?> GetAsync(string workedTimesId)
    {
        var snapshot = await FirestoreOperationHelper.ExecuteAsync(() => _workedTimes.Document(workedTimesId).GetSnapshotAsync());
        return snapshot.Exists ? snapshot.ConvertTo<WorkedTimes>() : null;
    }

    // SELECT ALL
    public async Task<List<(string Id, WorkedTimes WorkedTime)>> GetAllAsync()
    {
        var list = new List<(string, WorkedTimes)>();
        var snap = await FirestoreOperationHelper.ExecuteAsync(() => _workedTimes.GetSnapshotAsync());
        foreach (var doc in snap.Documents)
        {
            if (doc.Exists)
            {
                list.Add((doc.Id, doc.ConvertTo<WorkedTimes>()));
            }
        }
        return list;
    }

    // UPDATE
    public async Task UpdateAsync(string workedTimesId, WorkedTimes workedTimes)
    {
        await FirestoreOperationHelper.ExecuteAsync(() => _workedTimes.Document(workedTimesId).SetAsync(workedTimes, SetOptions.MergeAll));
    }

    // DELETE
    public async Task DeleteAsync(string workedTimesId)
    {
        await FirestoreOperationHelper.ExecuteAsync(() => _workedTimes.Document(workedTimesId).DeleteAsync());
    }
}