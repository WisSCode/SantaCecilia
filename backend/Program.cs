using backend.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5191");


//Cargar credenciales de Firebase
    var credential = CredentialFactory.FromFile<ServiceAccountCredential>("firebase-key.json")
        .ToGoogleCredential();

//Inicializar Firebase Admin
FirebaseApp.Create(new AppOptions
{
    Credential = credential,
    ProjectId = "santa-cecilia-s"
});

//Firestore con credenciales explicitas
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

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WorkerService>();
builder.Services.AddScoped<WorkTypeService>();
builder.Services.AddScoped<BatchService>();
builder.Services.AddScoped<WorkedTimeService>();
builder.Services.AddScoped<PayrollService>();
builder.Services.AddScoped<AuditLogService>();




var app = builder.Build();

// 🔹 Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Usar CORS
app.UseCors("AllowMAUI");

app.MapControllers();
app.Run();
