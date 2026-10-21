using RoyalDining.Desktop.Services;
using RoyalDining.Desktop.ViewModels;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace RoyalDining.Desktop.Views.Auth;

public partial class RegisterView : Window
{
    private readonly RegisterViewModel _viewModel;
    private bool _isManuallyMaximized = false;
    private double _prevLeft, _prevTop, _prevWidth, _prevHeight;

    public RegisterView()
    {
        InitializeComponent();
        _viewModel = new RegisterViewModel();
        DataContext = _viewModel;

        _viewModel.RegisterSuccess += () =>
        {
            SessionService.IsLastWindowMaximized = _isManuallyMaximized;
            var login = new LoginView();
            if (_isManuallyMaximized) login.WindowState = WindowState.Maximized;
            login.Show();
            Close();
        };

        this.Loaded += (s, e) =>
        {
            if (SessionService.IsLastWindowMaximized) ApplyMaximize();
        };
    }

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm) vm.Password = TxtPassword.Password;
    }

    private void TxtConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm) vm.ConfirmPassword = TxtConfirmPassword.Password;
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) BtnMaximize_Click(sender, e);
        else if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
    {
        if (!_isManuallyMaximized) ApplyMaximize();
        else RestoreWindow();
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

    private void GoToLogin_Click(object sender, RoutedEventArgs e)
    {
        SessionService.IsLastWindowMaximized = _isManuallyMaximized;
        var login = new LoginView();
        if (_isManuallyMaximized) login.WindowState = WindowState.Maximized;
        login.Show();
        Close();
    }
}