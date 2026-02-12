using Google.Cloud.Firestore;

namespace backend.Models;

[FirestoreData]
public class AuditLog
{
    [FirestoreProperty] public required string Action { get; set; }
    [FirestoreProperty] public required string Entity { get; set; }
    [FirestoreProperty] public required string EntityId { get; set; }
    [FirestoreProperty] public required string ActorId { get; set; }
    [FirestoreProperty] public required string Message { get; set; }
    [FirestoreProperty] public required Timestamp CreatedAt { get; set; }
}
