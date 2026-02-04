using backend.Models;
using backend.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);
var firebaseconfig = builder.Configuration.GetSection("Firebase");

//FirebaseAdmin
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile(firebaseconfig["credentialsPath"])
});

//FirestoreAccess
builder.Services.AddSingleton(Provider =>
{
    return FirestoreDb.Create(firebaseconfig["ProjectId"]);
});

//Controllers
builder.Services.AddControllers();

//Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WorkerService>();
builder.Services.AddScoped<WorkTypeService>();
builder.Services.AddScoped<BatchService>();
builder.Services.AddScoped<WorkedTimeService>();
builder.Services.AddScoped<PayrollService>();


var app = builder.Build();
app.MapControllers();
app.Run();
