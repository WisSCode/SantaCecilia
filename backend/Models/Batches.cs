using Google.Cloud.Firestore;
namespace backend.Models;

[FirestoreData]
public class Batches
{
    [FirestoreProperty] public required string Name { get; set; }
    [FirestoreProperty] public required string Location { get; set; }
}