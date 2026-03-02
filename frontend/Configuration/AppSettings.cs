namespace frontend.Configuration;

public static class AppSettings
{
    // URL del backend
    public static string BackendUrl
    {
        get
        {
    #if ANDROID
            var androidOverride = Environment.GetEnvironmentVariable("SC_BACKEND_URL_ANDROID");
            if (!string.IsNullOrWhiteSpace(androidOverride))
                return androidOverride;

            return DeviceInfo.DeviceType == DeviceType.Virtual
                ? "http://10.0.2.2:5191"
                : "http://192.168.30.131:5191";
    #elif WINDOWS
            var windowsOverride = Environment.GetEnvironmentVariable("SC_BACKEND_URL_WINDOWS");
            return !string.IsNullOrWhiteSpace(windowsOverride)
                ? windowsOverride
                : "http://localhost:5191";
    #else
            var backendOverride = Environment.GetEnvironmentVariable("SC_BACKEND_URL");
            return !string.IsNullOrWhiteSpace(backendOverride)
                ? backendOverride
                : "http://localhost:5191";
    #endif
        }
    }

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