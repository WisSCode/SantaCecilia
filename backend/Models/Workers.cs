using Google.Cloud.Firestore;
namespace backend.Models;

[FirestoreData]
public class Workers
{
    [FirestoreProperty] public string? UserId { get; set; }
    [FirestoreProperty] public required string Name { get; set; }
    [FirestoreProperty] public required string LastName { get; set; }
    [FirestoreProperty] public required string Identification { get; set; }
    [FirestoreProperty] public required bool Active { get; set; }
}