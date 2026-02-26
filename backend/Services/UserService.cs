using Google.Cloud.Firestore;
using backend.Models;
namespace backend.Services;

public class UserService
{
    private readonly CollectionReference _users;
    public UserService(FirestoreDb db)
    {
        _users = db.Collection("users");
    }

    // CREATE
    public async Task CreateAsync(string userId, Users user)
    {
        await FirestoreOperationHelper.ExecuteAsync(() => _users.Document(userId).SetAsync(user));
    }

    // SELECT
    public async Task<Users?> GetAsync(string userId)
    {
        var snapshot = await FirestoreOperationHelper.ExecuteAsync(() => _users.Document(userId).GetSnapshotAsync());
        return snapshot.Exists ? snapshot.ConvertTo<Users>() : null;
    }

    // SELECT ALL
    public async Task<List<(string Id, Users User)>> GetAllAsync()
    {
        var list = new List<(string, Users)>();
        var snap = await FirestoreOperationHelper.ExecuteAsync(() => _users.GetSnapshotAsync());
        foreach (var doc in snap.Documents)
        {
            if (doc.Exists)
            {
                list.Add((doc.Id, doc.ConvertTo<Users>()));
            }
        }
        return list;
    }

    // UPDATE
    public async Task UpdateAsync(string userId, Users user)
    {
        await FirestoreOperationHelper.ExecuteAsync(() => _users.Document(userId).SetAsync(user, SetOptions.MergeAll));
    }
}