using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using frontend.Services;
using frontend.Models.Auth;
using frontend.Configuration;

namespace frontend.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;
    private readonly SessionService _sessionService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public LoginViewModel(AuthService authService, SessionService sessionService)
    {
        _authService = authService;
        _sessionService = sessionService;
        LoginCommand = new Command(async () => await ExecuteLoginAsync(), () => !IsBusy);
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            ((Command)LoginCommand).ChangeCanExecute();
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

    public ICommand LoginCommand { get; }

    public async Task<AuthResponse?> ExecuteLoginAsync()
    {
        if (IsBusy) return null;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor ingresa email y contraseña";
            return null;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _authService.LoginAsync(Email, Password);
            
            if (result != null)
            {
                // Guardar sesión usando SessionService
                await _sessionService.SaveSessionAsync(result);
            }

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