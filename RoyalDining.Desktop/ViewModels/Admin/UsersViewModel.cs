using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using RoyalDining.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace RoyalDining.Desktop.ViewModels.Admin;

public partial class UsersViewModel : ObservableObject
{
    public ObservableCollection<User> Users { get; } = new();

    [ObservableProperty] private bool isBusy;

    public UsersViewModel()
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsBusy = true;
        Users.Clear();

        using var db = new AppDbContext();
        var list = await db.Users.OrderBy(u => u.Id).ToListAsync();

        foreach (var u in list) Users.Add(u);

        IsBusy = false;
    }

    [RelayCommand]
    private async Task DeleteUserAsync(User user)
    {
        if (user == null) return;

        // Proteksi: tidak bisa hapus diri sendiri
        if (SessionService.CurrentUser?.Id == user.Id)
        {
            MessageBox.Show("Anda tidak bisa menghapus akun Anda sendiri yang sedang digunakan.",
                "Tidak Bisa Dihapus", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show($"Yakin ingin menghapus user '{user.Name}' ({user.Email})?",
            "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new AppDbContext();

                // Cek apakah user punya order
                var orderCount = await db.Orders.CountAsync(o => o.UserId == user.Id);
                if (orderCount > 0)
                {
                    MessageBox.Show($"User '{user.Name}' tidak bisa dihapus karena memiliki {orderCount} pesanan.",
                        "Tidak Bisa Dihapus", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var entity = await db.Users.FindAsync(user.Id);
                if (entity != null)
                {
                    db.Users.Remove(entity);
                    await db.SaveChangesAsync();
                }
                Users.Remove(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menghapus: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}