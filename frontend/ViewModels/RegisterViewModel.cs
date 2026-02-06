using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using frontend.Services;
using frontend.Models.Auth;

namespace frontend.ViewModels;

public class RegisterViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public RegisterViewModel(AuthService authService)
    {
        _authService = authService;
        RegisterCommand = new Command(async () => await ExecuteRegisterAsync(), () => !IsBusy);
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand RegisterCommand { get; }

    public async Task<AuthResponse?> ExecuteRegisterAsync()
    {
        if (IsBusy) return null;

        // Validaciones
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "El email es requerido";
            return null;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "La contraseña es requerida";
            return null;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "La contraseña debe tener al menos 6 caracteres";
            return null;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _authService.RegisterAsync(Email, Password);
            
            return result;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
