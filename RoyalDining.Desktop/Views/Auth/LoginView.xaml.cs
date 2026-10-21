using RoyalDining.Desktop.Services;
using RoyalDining.Desktop.ViewModels;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace RoyalDining.Desktop.Views.Auth;

public partial class LoginView : Window
{
    private readonly LoginViewModel _viewModel;
    private bool _isManuallyMaximized = false;
    private double _prevLeft, _prevTop, _prevWidth, _prevHeight;

    public LoginView()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        DataContext = _viewModel;

        _viewModel.LoginSuccess += OnLoginSuccess;

        // Terapkan state window terakhir
        this.Loaded += (s, e) =>
        {
            if (SessionService.IsLastWindowMaximized)
            {
                ApplyMaximize();
            }
        };
    }

    private void OnLoginSuccess(Models.User user)
    {
        SessionService.LastWindowState = this.WindowState;
        SessionService.IsLastWindowMaximized = _isManuallyMaximized; // Pastikan ini tersimpan

        var shell = new ShellWindow();
        // Baris di bawah ini tidak perlu lagi, karena ShellWindow sudah handle sendiri di konstruktor
        // if (_isManuallyMaximized) shell.WindowState = WindowState.Maximized; 

        shell.Show();
        Close();
    }

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = TxtPassword.Password;
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            BtnMaximize_Click(sender, e);
        }
        else if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
    {
        if (!_isManuallyMaximized)
            ApplyMaximize();
        else
            RestoreWindow();
    }

    private void ApplyMaximize()
    {
        _prevLeft = this.Left;
        _prevTop = this.Top;
        _prevWidth = this.Width;
        _prevHeight = this.Height;

        var workArea = SystemParameters.WorkArea;
        this.WindowState = WindowState.Normal;
        this.Left = workArea.Left;
        this.Top = workArea.Top;
        this.Width = workArea.Width;
        this.Height = workArea.Height;

        _isManuallyMaximized = true;
        IconMaximize.Kind = PackIconKind.WindowRestore;
    }

    private void RestoreWindow()
    {
        this.Left = _prevLeft;
        this.Top = _prevTop;
        this.Width = _prevWidth;
        this.Height = _prevHeight;

        _isManuallyMaximized = false;
        IconMaximize.Kind = PackIconKind.WindowMaximize;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    private void GoToRegister_Click(object sender, RoutedEventArgs e)
    {
        SessionService.IsLastWindowMaximized = _isManuallyMaximized;

        var registerView = new RegisterView();
        if (_isManuallyMaximized)
            registerView.WindowState = WindowState.Maximized;

        registerView.Show();
        Close();
    }
}