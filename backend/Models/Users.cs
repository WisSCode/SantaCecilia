using Google.Cloud.Firestore;
namespace backend.Models;

[FirestoreData]
public class Users
{
    [FirestoreProperty] public required string Email { get; set; }
    [FirestoreProperty] public required string Role { get; set; }
    [FirestoreProperty] public required bool Validated { get; set; }
    [FirestoreProperty] public Timestamp CreatedAt { get; set; }
}