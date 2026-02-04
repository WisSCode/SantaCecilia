using Google.Cloud.Firestore;
namespace backend.Models;

[FirestoreData]
public class WorkTypes
{
    [FirestoreProperty] public required string Name { get; set; }
    [FirestoreProperty] public required double DefaultRate { get; set; }
}