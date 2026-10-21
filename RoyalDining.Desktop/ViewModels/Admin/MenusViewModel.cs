using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows;
using MenuModel = RoyalDining.Desktop.Models.Menu; // Alias agar tidak bentrok

namespace RoyalDining.Desktop.ViewModels.Admin;

public partial class MenusViewModel : ObservableObject
{
    public ObservableCollection<MenuModel> Menus { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty] private bool isBusy;

    public MenusViewModel()
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsBusy = true;
        Menus.Clear();
        Categories.Clear();

        using var db = new AppDbContext();

        // Load Categories
        var cats = await db.Categories.OrderBy(c => c.Name).ToListAsync();
        foreach (var c in cats) Categories.Add(c);

        // Load Menus
        var list = await db.Menus
            .Include(m => m.Category)
            .OrderBy(m => m.Id)
            .ToListAsync();

        foreach (var m in list) Menus.Add(m);

        IsBusy = false;
    }

    [RelayCommand]
    private async Task DeleteMenuAsync(MenuModel menu)
    {
        if (menu == null) return;

        try
        {
            using var db = new AppDbContext();

            var usedCount = await db.OrderItems.CountAsync(oi => oi.MenuId == menu.Id);
            if (usedCount > 0)
            {
                MessageBox.Show($"Menu '{menu.Name}' tidak bisa dihapus karena sudah pernah dipesan {usedCount} kali.",
                    "Tidak Bisa Dihapus", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Yakin ingin menghapus menu '{menu.Name}'?",
                "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                var entity = await db.Menus.FindAsync(menu.Id);
                if (entity != null)
                {
                    db.Menus.Remove(entity);
                    await db.SaveChangesAsync();
                }

                Menus.Remove(menu);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal menghapus: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}