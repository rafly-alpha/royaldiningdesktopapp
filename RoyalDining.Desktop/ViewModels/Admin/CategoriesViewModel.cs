using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace RoyalDining.Desktop.ViewModels.Admin;

public partial class CategoriesViewModel : ObservableObject
{
    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty] private bool isBusy;

    public CategoriesViewModel()
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsBusy = true;
        Categories.Clear();

        using var db = new AppDbContext();
        var list = await db.Categories.OrderBy(c => c.Id).ToListAsync();

        foreach (var c in list)
            Categories.Add(c);

        IsBusy = false;
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(Category category)
    {
        if (category == null) return;

        try
        {
            using var db = new AppDbContext();

            // Cek apakah kategori dipakai oleh menu
            var usedCount = await db.Menus.CountAsync(m => m.CategoryId == category.Id);
            if (usedCount > 0)
            {
                MessageBox.Show($"Kategori '{category.Name}' tidak bisa dihapus karena masih digunakan oleh {usedCount} menu.",
                    "Tidak Bisa Dihapus", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Yakin ingin menghapus kategori '{category.Name}'?",
                "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                var entity = await db.Categories.FindAsync(category.Id);
                if (entity != null)
                {
                    db.Categories.Remove(entity);
                    await db.SaveChangesAsync();
                }

                Categories.Remove(category);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal menghapus: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}