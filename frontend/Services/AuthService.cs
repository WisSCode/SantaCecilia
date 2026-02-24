using System.Net.Http.Json;
using frontend.Models.Auth;
using frontend.Configuration;

namespace frontend.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _firebaseClient;
    private readonly string _firebaseApiKey = AppSettings.FirebaseApiKey;
    private readonly string _backendUrl = AppSettings.BackendUrl;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _firebaseClient = new HttpClient();
    }

    public async Task<AuthResponse?> RegisterAsync(string email, string password)
    {
        try
        {
            var registerDto = new RegisterRequest
            {
                Email = email,
                Password = password
            };

            var url = $"{_backendUrl}/api/auth/register";
            var response = await _httpClient.PostAsJsonAsync(url, registerDto);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorObj = TryParseError(errorContent);
                throw new Exception(errorObj);
            }

            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"No se pudo conectar al servidor: {ex.Message}");
        }
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        try
        {
            // 1. Autenticar con Firebase Auth
            var firebaseUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}";
            
            var firebasePayload = new
            {
                email,
                password,
                returnSecureToken = true
            };

            Console.WriteLine($"[AuthService] Intentando login con Firebase para: {email}");
            var firebaseResponse = await _firebaseClient.PostAsJsonAsync(firebaseUrl, firebasePayload);

            if (!firebaseResponse.IsSuccessStatusCode)
            {
                var error = await firebaseResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[AuthService] Error de Firebase: {error}");
                
                // Parsear el error de Firebase para un mensaje más amigable
                if (error.Contains("INVALID_LOGIN_CREDENTIALS") || error.Contains("INVALID_PASSWORD"))
                {
                    throw new Exception("Email o contraseña incorrectos");
                }
                else if (error.Contains("USER_DISABLED"))
                {
                    throw new Exception("Esta cuenta ha sido deshabilitada");
                }
                else if (error.Contains("EMAIL_NOT_FOUND"))
                {
                    throw new Exception("No existe una cuenta con este email");
                }
                else if (error.Contains("TOO_MANY_ATTEMPTS"))
                {
                    throw new Exception("Demasiados intentos fallidos. Intenta más tarde");
                }
                
                throw new Exception($"Error de autenticación: {error}");
            }


            var firebaseData = await firebaseResponse.Content.ReadFromJsonAsync<FirebaseLoginResponse>();
            
            if (firebaseData == null || string.IsNullOrEmpty(firebaseData.IdToken))
            {
                Console.WriteLine("[AuthService] No se pudo obtener el token de Firebase");
                throw new Exception("No se pudo obtener el token de Firebase");
            }

            Console.WriteLine("[AuthService] Token de Firebase obtenido exitosamente");

            // 2. Validar token con backend
            var backendPayload = new { idToken = firebaseData.IdToken };
            var url = $"{_backendUrl}/api/auth/login";
            var backendResponse = await _httpClient.PostAsJsonAsync(url, backendPayload);
            
            if (!backendResponse.IsSuccessStatusCode)
            {
                var errorContent = await backendResponse.Content.ReadAsStringAsync();
                var errorObj = TryParseError(errorContent);
                Console.WriteLine($"[AuthService] Error del backend: {errorObj}");
                throw new Exception(errorObj);
            }

            var authResponse = await backendResponse.Content.ReadFromJsonAsync<AuthResponse>();
            
            if (authResponse != null)
                authResponse.Token = firebaseData.IdToken;

            Console.WriteLine($"[AuthService] Login exitoso para: {email}");
            return authResponse;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[AuthService] Error de conexión: {ex.Message}");
            throw new Exception($"No se pudo conectar al servidor: {ex.Message}");
        }
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        var url =
            $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_firebaseApiKey}";

        var payload = new
        {
            requestType = "PASSWORD_RESET",
            email
        };

        var response = await _firebaseClient.PostAsJsonAsync(url, payload);

        if (!response.IsSuccessStatusCode)
            throw new Exception("No se pudo enviar el correo de restablecimiento");
    }


    private string TryParseError(string errorContent)
    {
        try
        {
            var errorObj = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(errorContent);
            return errorObj?.Message ?? errorContent;
        }
        catch
        {
            return errorContent;
        }
    }
}

public class FirebaseLoginResponse
{
    public string IdToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string LocalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}