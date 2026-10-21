using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoyalDining.Desktop.Services;
using RoyalDining.Desktop.Models;

namespace RoyalDining.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private bool isBusy = false;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    public event Action<User>? LoginSuccess;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        var result = await AuthService.LoginAsync(Email, Password);
        IsBusy = false;

        if (result.Success && result.User != null)
            LoginSuccess?.Invoke(result.User);
        else
            ErrorMessage = result.Message;
    }
}