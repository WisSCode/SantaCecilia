using backend.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

//Cargar credenciales de Firebase
var credential = GoogleCredential.FromFile("firebase-key.json");

//Inicializar Firebase Admin
FirebaseApp.Create(new AppOptions
{
    Credential = credential,
    ProjectId = "santa-cecilia-s"
});

//Firestore con credenciales explícitas
builder.Services.AddSingleton(provider =>
{
    return new FirestoreDbBuilder
    {
        ProjectId = "santa-cecilia-s",
        Credential = credential
    }.Build();
});

// CORS - Permitir que el frontend se comunique con el backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMAUI", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Controllers
builder.Services.AddControllers();

// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WorkerService>();
builder.Services.AddScoped<WorkTypeService>();
builder.Services.AddScoped<BatchService>();
builder.Services.AddScoped<WorkedTimeService>();
builder.Services.AddScoped<PayrollService>();

var app = builder.Build();

// Usar CORS
app.UseCors("AllowMAUI");

app.MapControllers();
app.Run();
