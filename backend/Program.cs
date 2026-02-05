using backend.Models;
using backend.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);
var firebaseconfig = builder.Configuration.GetSection("Firebase");

//FirebaseAdmin (only initialize if credentials file exists)
var credentialsPath = firebaseconfig["CredentialsPath"] ?? firebaseconfig["credentialsPath"];
if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
{
    try
    {
        using (var stream = File.OpenRead(credentialsPath))
        {
            var credential = GoogleCredential.FromStream(stream);
            FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: failed to initialize FirebaseAdmin: {ex.Message}");
    }
}
else
{
    Console.WriteLine("Warning: Firebase credentials file not found, skipping Firebase initialization.");
}

//FirestoreAccess
builder.Services.AddSingleton(Provider =>
{
    return FirestoreDb.Create(firebaseconfig["ProjectId"]);
});

//Controllers
builder.Services.AddControllers();

// CORS - allow local development origins (adjust for production)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

//Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WorkerService>();
builder.Services.AddScoped<WorkTypeService>();
builder.Services.AddScoped<BatchService>();
builder.Services.AddScoped<WorkedTimeService>();
builder.Services.AddScoped<PayrollService>();


var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

app.MapControllers();
app.Run();
