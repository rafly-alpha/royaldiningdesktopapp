using RoyalDining.Desktop.Services;
using RoyalDining.Desktop.Views.Admin;
using RoyalDining.Desktop.Views.Customer;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace RoyalDining.Desktop.Views;

public partial class ShellWindow : Window
{
    private Button? _activeButton;
    private bool _isManuallyMaximized = false; // TAMBAHKAN INI
    private double _prevLeft, _prevTop, _prevWidth, _prevHeight; // TAMBAHKAN INI

    public ShellWindow()
    {
        InitializeComponent();

        // 1. Simpan ukuran default SEBELUM maximize
        _prevWidth = this.Width;   // Biasanya 1400 dari XAML
        _prevHeight = this.Height; // Biasanya 800 dari XAML
        _prevLeft = this.Left;
        _prevTop = this.Top;

        // 2. Terapkan Maximize Manual jika state terakhir maximize
        if (SessionService.IsLastWindowMaximized)
        {
            var workArea = SystemParameters.WorkArea;
            this.WindowState = WindowState.Normal;
            this.Left = workArea.Left;
            this.Top = workArea.Top;
            this.Width = workArea.Width;
            this.Height = workArea.Height;
            IconMaximize.Kind = PackIconKind.WindowRestore;
            _isManuallyMaximized = true;
        }

        // 3. Init User
        var user = SessionService.CurrentUser;
        if (user != null)
        {
            TxtUserName.Text = user.Name;
            TxtUserRole.Text = user.Role;
            TxtInitials.Text = GetInitials(user.Name);
        }

        // 4. Build Menu
        BuildMenu();
        NavigationService.OnNavigate = (page) => MainContent.Content = page;
        NavigateToDashboard();
    }

    // ============ MENU BUILDER ============

    private void BuildMenu()
    {
        if (SessionService.IsAdmin)
        {
            AddMenuItem("Dashboard", PackIconKind.ViewDashboard, () => new AdminDashboardPage());
            AddMenuItem("Kategori", PackIconKind.TagMultiple, () => new CategoriesPage());
            AddMenuItem("Menu", PackIconKind.Food, () => new MenusPage());
            AddMenuItem("Pesanan", PackIconKind.ReceiptText, () => new OrdersPage());
            AddMenuItem("Laporan", PackIconKind.ChartBar, () => new ReportsPage());
            AddMenuItem("Pengguna", PackIconKind.AccountGroup, () => new UsersPage());
        }
        else
        {
            AddMenuItem("Dashboard", PackIconKind.Home, () => new CustomerDashboardPage());
            AddMenuItem("Pesan Sekarang", PackIconKind.CartPlus, () => new OrderingPage());
            AddMenuItem("Pesanan Saya", PackIconKind.ClipboardList, () => new MyOrdersPage());
            AddMenuItem("Riwayat", PackIconKind.History, () => new HistoryPage());
        }
    }

    private void AddMenuItem(string title, PackIconKind icon, Func<UserControl> pageFactory)
    {
        var btn = new Button
        {
            Style = (Style)FindResource("MenuItemButtonStyle"),
            Tag = new MenuItemData { Title = title, PageFactory = pageFactory, IconKind = icon },
            Content = CreateMenuItemContent(title, icon),
            Margin = new Thickness(0, 2, 0, 2),
            HorizontalContentAlignment = HorizontalAlignment.Left
        };
        btn.Click += MenuItem_Click;

        MenuStackPanel.Children.Add(btn);
    }

    private StackPanel CreateMenuItemContent(string title, PackIconKind icon)
    {
        var sp = new StackPanel { Orientation = Orientation.Horizontal };

        var iconEl = new PackIcon
        {
            Kind = icon,
            Width = 24,  // Diperbesar dari 20
            Height = 24, // Diperbesar dari 20
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 15, 0) // Jarak ikon ke teks diperlebar
        };
        iconEl.SetResourceReference(PackIcon.ForegroundProperty, "TextSecondaryBrush");

        var textEl = new TextBlock
        {
            Text = title,
            FontSize = 16, // Diperbesar dari 14
            FontWeight = FontWeights.SemiBold, // Dibuat semi-bold agar tegas
            VerticalAlignment = VerticalAlignment.Center
        };
        textEl.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

        sp.Children.Add(iconEl);
        sp.Children.Add(textEl);
        return sp;
    }

    // ============ NAVIGATION HANDLER ============

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not MenuItemData data) return;

        if (_activeButton != null)
        {
            _activeButton.ClearValue(Button.BackgroundProperty);
        }

        btn.SetResourceReference(Button.BackgroundProperty, "SurfaceAltBrush");
        _activeButton = btn;

        TxtPageTitle.Text = data.Title;
        NavigationService.Navigate(data.PageFactory());
    }

    private void NavigateToDashboard()
    {
        if (MenuStackPanel.Children.Count > 0 && MenuStackPanel.Children[0] is Button firstBtn)
        {
            firstBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
    }

    // ============ HELPER ============

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
    }

    // ============ WINDOW CONTROLS ============

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double-click = toggle maximize
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
        {
            // Simpan ukuran saat ini sebelum maximize
            _prevLeft = this.Left;
            _prevTop = this.Top;
            _prevWidth = this.Width;
            _prevHeight = this.Height;

            // Maximize manual
            var workArea = SystemParameters.WorkArea;
            this.WindowState = WindowState.Normal;
            this.Left = workArea.Left;
            this.Top = workArea.Top;
            this.Width = workArea.Width;
            this.Height = workArea.Height;

            _isManuallyMaximized = true;
            IconMaximize.Kind = PackIconKind.WindowRestore;
        }
        else
        {
            // Restore ke ukuran sebelumnya
            this.Left = _prevLeft;
            this.Top = _prevTop;
            this.Width = _prevWidth;
            this.Height = _prevHeight;

            _isManuallyMaximized = false;
            IconMaximize.Kind = PackIconKind.WindowMaximize;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    // ============ THEME TOGGLE ============

    private void BtnTheme_Click(object sender, RoutedEventArgs e)
    {
        var newTheme = ThemeService.Toggle();
        IconTheme.Kind = newTheme == ThemeService.ThemeMode.Dark ? PackIconKind.WeatherSunny : PackIconKind.WeatherNight;
    }

    // ============ LOGOUT ============

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Yakin ingin logout?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            SessionService.Logout();
            var login = new Auth.LoginView();
            login.Show();
            Close();
        }
    }
}

// Class untuk data menu
public class MenuItemData
{
    public string Title { get; set; } = string.Empty;
    public PackIconKind IconKind { get; set; }
    public Func<UserControl> PageFactory { get; set; } = () => new UserControl();
}