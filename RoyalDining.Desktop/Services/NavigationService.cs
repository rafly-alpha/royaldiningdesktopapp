using System.Windows.Controls;

namespace RoyalDining.Desktop.Services;

public static class NavigationService
{
    // Action yang dipanggil ShellWindow untuk mengganti halaman
    public static Action<UserControl>? OnNavigate;

    /// <summary>Pindah ke halaman lain (UserControl).</summary>
    public static void Navigate(UserControl page)
    {
        OnNavigate?.Invoke(page);
    }
}