using Google.Cloud.Firestore;
using backend.Models;

namespace backend.Services;

public class AuditLogService
{
    private readonly CollectionReference _logs;

    public AuditLogService(FirestoreDb db)
    {
        _logs = db.Collection("auditLogs");
    }

    public async Task CreateAsync(AuditLog log)
    {
        await _logs.AddAsync(log);
    }

    public async Task<List<(string Id, AuditLog Log)>> GetAllAsync(int limit = 200)
    {
        var list = new List<(string, AuditLog)>();
        var query = _logs.OrderByDescending("CreatedAt").Limit(limit);
        var snap = await query.GetSnapshotAsync();
        foreach (var doc in snap.Documents)
        {
            if (doc.Exists)
            {
                list.Add((doc.Id, doc.ConvertTo<AuditLog>()));
            }
        }
        return list;
    }
}
