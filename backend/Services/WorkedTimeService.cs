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
        await _workedTimes.Document(workedTimeId).SetAsync(workedTimes);
    }

    // SELECT
    public async Task<WorkedTimes?> GetAsync(string workedTimesId)
    {
        var snapshot = await _workedTimes.Document(workedTimesId).GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<WorkedTimes>() : null;
    }

    // SELECT ALL
    public async Task<List<WorkedTimes>> GetAllAsync()
    {
        var list = new List<WorkedTimes>();
        var snap = await _workedTimes.GetSnapshotAsync();
        foreach (var doc in snap.Documents)
        {
            if (doc.Exists)
            {
                list.Add(doc.ConvertTo<WorkedTimes>());
            }
        }
        return list;
    }

    // UPDATE
    public async Task UpdateAsync(string workedTimesId, WorkedTimes workedTimes)
    {
        await _workedTimes.Document(workedTimesId).SetAsync(workedTimes, SetOptions.MergeAll);
    }
}