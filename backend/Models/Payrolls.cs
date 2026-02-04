using Google.Cloud.Firestore;
namespace backend.Models;

[FirestoreData]
public class Payrolls
{
    [FirestoreProperty] public required string WorkerId { get; set; }
    [FirestoreProperty] public required Timestamp WeekStart { get; set; }
    [FirestoreProperty] public required Timestamp WeekEnd { get; set; }
    [FirestoreProperty] public required int TotalMinutes { get; set; }
    [FirestoreProperty] public required double GrossAmount { get; set; }
    [FirestoreProperty] public required string Status { get; set; }
    [FirestoreProperty] public Timestamp? PaidAt { get; set; }
}