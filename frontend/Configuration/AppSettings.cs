namespace frontend.Configuration;

public static class AppSettings
{
    // URL del backend en Render
    public static string BackendUrl => "render_url_here";

    // Firebase Web API Key
    public static string FirebaseApiKey => "firebase_api_key_here";

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