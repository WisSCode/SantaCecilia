using frontend.Configuration;
using frontend.Models.Auth;

namespace frontend.Services;

public class SessionService
{
    public bool IsLoggedIn { get; private set; }
    public string? UserId { get; private set; }
    public string? Email { get; private set; }
    public string? Role { get; private set; }
    public string? Token { get; private set; }

    public event EventHandler<bool>? AuthStateChanged;

    // Guardar sesión
    public async Task SaveSessionAsync(AuthResponse authResponse)
    {
        await SecureStorage.SetAsync(AppSettings.StorageKeys.UserToken, authResponse.Token);
        await SecureStorage.SetAsync(AppSettings.StorageKeys.UserId, authResponse.UserId);
        await SecureStorage.SetAsync(AppSettings.StorageKeys.UserEmail, authResponse.Email);
        await SecureStorage.SetAsync(AppSettings.StorageKeys.UserRole, authResponse.Role);

        UserId = authResponse.UserId;
        Email = authResponse.Email;
        Role = authResponse.Role;
        Token = authResponse.Token;
        IsLoggedIn = true;

        AuthStateChanged?.Invoke(this, true);
    }

    // Cargar sesión guardada
    public async Task<bool> LoadSessionAsync()
    {
        try
        {
            Token = await SecureStorage.GetAsync(AppSettings.StorageKeys.UserToken);
            UserId = await SecureStorage.GetAsync(AppSettings.StorageKeys.UserId);
            Email = await SecureStorage.GetAsync(AppSettings.StorageKeys.UserEmail);
            Role = await SecureStorage.GetAsync(AppSettings.StorageKeys.UserRole);

            IsLoggedIn = !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(UserId);
            
            return IsLoggedIn;
        }
        catch
        {
            return false;
        }
    }

    // Cerrar sesión
    public async Task LogoutAsync()
    {
        SecureStorage.Remove(AppSettings.StorageKeys.UserToken);
        SecureStorage.Remove(AppSettings.StorageKeys.UserId);
        SecureStorage.Remove(AppSettings.StorageKeys.UserEmail);
        SecureStorage.Remove(AppSettings.StorageKeys.UserRole);

        UserId = null;
        Email = null;
        Role = null;
        Token = null;
        IsLoggedIn = false;

        AuthStateChanged?.Invoke(this, false);
    }

    // Verificar si el usuario tiene un rol específico
    public bool HasRole(string role)
    {
        return IsLoggedIn && Role?.Equals(role, StringComparison.OrdinalIgnoreCase) == true;
    }
}
