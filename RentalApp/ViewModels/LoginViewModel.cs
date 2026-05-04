using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Services;

namespace RentalApp.ViewModels;

// Handles login and navigation to register
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isBusy;

    public LoginViewModel()
    {
        // Default constructor for design time support
        Title = "Login";
    }

    public LoginViewModel(IAuthenticationService authService, INavigationService navigationService, IApiService apiService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = apiService;
        Title = "Login";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter both email and password");
            return;
        }

        try
        {
            IsBusy = true;
            ResetError();

            // 1. Try local DB login — skip if DB is unavailable
            var localResult = await _authService.LoginAsync(Email, Password);
            bool localAuthAvailable = localResult.IsSuccess ||
                (!localResult.Message.Contains("transient") && !localResult.Message.Contains("connection"));

            if (localAuthAvailable && !localResult.IsSuccess)
            {
                SetError(localResult.Message);
                return;
            }

            // 2. Gets the JWT token from the API
            var apiResult = await _apiService.LoginAsync(Email, Password);

            if (apiResult == null)
            {
                // User exists locally but not on API = register user
                await _apiService.RegisterAsync(
                    _authService.CurrentUser?.FirstName ?? "",
                    _authService.CurrentUser?.LastName ?? "",
                    Email,
                    Password);

                // Trys to login again
                apiResult = await _apiService.LoginAsync(Email, Password);
            }

            if (apiResult == null)
            {
                SetError("Login failed: could not authenticate with server");
                return;
            }

            await _navigationService.NavigateToAsync("MainPage");
        }
        catch (Exception ex)
        {
            SetError($"Login failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await _navigationService.NavigateToAsync("RegisterPage");
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        // Implement forgot password functionality remember
        await Application.Current.MainPage.DisplayAlert("Info", "Forgot password functionality not implemented yet", "OK");
    }
}
