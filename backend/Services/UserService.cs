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
        await _users.Document(userId).SetAsync(user);
    }

    // SELECT
    public async Task<Users?> GetAsync(string userId)
    {
        var snapshot = await _users.Document(userId).GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<Users>() : null;
    }

    // SELECT ALL
    public async Task<List<Users>> GetAllAsync()
    {
        var list = new List<Users>();
        var snap = await _users.GetSnapshotAsync();
        foreach (var doc in snap.Documents)
        {
            if (doc.Exists)
            {
                list.Add(doc.ConvertTo<Users>());
            }
        }
        return list;
    }

    // UPDATE
    public async Task UpdateAsync(string userId, Users user)
    {
        await _users.Document(userId).SetAsync(user, SetOptions.MergeAll);
    }
}