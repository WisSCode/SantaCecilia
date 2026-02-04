using Google.Cloud.Firestore;
namespace backend.Models;
[FirestoreData]
public class WorkedTimes
{
    [FirestoreProperty] public required string WorkerId { get; set; }
    [FirestoreProperty] public required string WorkTypeId { get; set; }
    [FirestoreProperty] public required string BatchId { get; set; }
    [FirestoreProperty] public required int MinutesWorked { get; set; }
    [FirestoreProperty] public required Timestamp date {  get; set; }
}