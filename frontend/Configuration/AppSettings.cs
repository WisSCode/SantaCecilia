namespace frontend.Configuration;

public static class AppSettings
{
    // URL del backend
    public static string BackendUrl => 
    #if DEBUG
        "http://localhost:5191";
    #else
        "https://tu-api-produccion.com"; // Reemplazar con la URL cuando se use en producción
    #endif

    // Firebase Web API Key
    public static string FirebaseApiKey => "AIzaSyBYPpwPUmbS89365RLBXSj6jlZkmB_arcg";
    
    // Claves para SecureStorage
    public static class StorageKeys
    {
        public const string UserToken = "user_token";
        public const string UserId = "user_id";
        public const string UserEmail = "user_email";
        public const string UserRole = "user_role";
    }
}
