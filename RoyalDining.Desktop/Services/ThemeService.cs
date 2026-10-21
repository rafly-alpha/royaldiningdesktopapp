using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace RoyalDining.Desktop.Services;

public static class ThemeService
{
    public enum ThemeMode { Light, Dark }

    public static ThemeMode CurrentTheme { get; private set; } = ThemeMode.Light;

    /// <summary>Ganti tema aplikasi secara live.</summary>
    public static void ApplyTheme(ThemeMode mode)
    {
        CurrentTheme = mode;
        var app = Application.Current;
        if (app == null) return;

        var dicts = app.Resources.MergedDictionaries;

        // Cari dictionary tema lama
        var oldTheme = dicts.FirstOrDefault(d =>
            d.Source != null &&
            (d.Source.OriginalString.Contains("LightTheme.xaml") ||
             d.Source.OriginalString.Contains("DarkTheme.xaml")));

        if (oldTheme != null)
            dicts.Remove(oldTheme);

        // Tambahkan tema baru
        var newUri = mode == ThemeMode.Dark
            ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

        dicts.Add(new ResourceDictionary { Source = newUri });
    }

    /// <summary>Toggle antara Light ↔ Dark.</summary>
    public static ThemeMode Toggle()
    {
        var next = CurrentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
        ApplyTheme(next);
        return next;
    }
}
