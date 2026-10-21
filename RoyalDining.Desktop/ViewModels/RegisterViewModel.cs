using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoyalDining.Desktop.Services;

namespace RoyalDining.Desktop.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private bool isBusy = false;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    public event Action? RegisterSuccess;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Konfirmasi kata sandi tidak cocok.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        var result = await AuthService.RegisterAsync(Name, Email, Password);
        IsBusy = false;

        if (result.Success)
        {
            System.Windows.MessageBox.Show("Registrasi berhasil! Silakan login.", "Sukses", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RegisterSuccess?.Invoke();
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }
}