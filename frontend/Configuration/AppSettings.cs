namespace frontend.Configuration;

public static class AppSettings
{
    // URL del backend
    public static string BackendUrl =>
    #if ANDROID
        "http://10.0.2.2:5191";
    #elif DEBUG
        "http://localhost:5191";
    #else
        "https://tu-api-produccion.com"; // Reemplazar con la URL cuando se use en produccion
    #endif

    // Firebase Web API Key
    public static string FirebaseApiKey => "TU_FIREBASE_API_KEY"; 
    
    // Claves para SecureStorage
    public static class StorageKeys
    {
        public const string UserToken = "user_token";
        public const string UserId = "user_id";
        public const string UserEmail = "user_email";
        public const string UserRole = "user_role";
    }

    public static class DevAutoLogin
    {
        #if DEBUG && ANDROID
            public const bool Enabled = true;
            public const string Email = "";
            public const string Password = "";
        #elif DEBUG && WINDOWS
            public const bool Enabled = true;
            public const string Email = "";
            public const string Password = "";
        #else
            public const bool Enabled = false;
            public const string Email = "";
            public const string Password = "";
        #endif
    }
}