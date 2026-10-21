using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using RoyalDining.Desktop.Services;

namespace RoyalDining.Desktop.Views;

public partial class ThemeTestWindow : Window
{
    public ThemeTestWindow()
    {
        InitializeComponent();
    }

    private void BtnToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        var newTheme = ThemeService.Toggle();

        if (newTheme == ThemeService.ThemeMode.Dark)
        {
            BtnToggleTheme.Content = "☀️ Ganti ke Light Mode";
        }
        else
        {
            BtnToggleTheme.Content = "🌙 Ganti ke Dark Mode";
        }
    }
}
