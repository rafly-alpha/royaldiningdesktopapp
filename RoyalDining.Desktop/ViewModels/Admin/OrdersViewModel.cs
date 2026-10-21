using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace RoyalDining.Desktop.ViewModels.Admin;

public partial class OrdersViewModel : ObservableObject
{
    public ObservableCollection<Order> Orders { get; } = new();

    [ObservableProperty] private bool isBusy;

    public OrdersViewModel()
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsBusy = true;
        Orders.Clear();

        using var db = new AppDbContext();
        var list = await db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Menu)
            .Where(o => o.Status != "Pending")
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        foreach (var o in list) Orders.Add(o);

        IsBusy = false;
    }

    [RelayCommand]
    private async Task DeleteOrderAsync(Order order)
    {
        if (order == null) return;

        var confirm = MessageBox.Show($"Yakin ingin menghapus pesanan #{order.Id}? Tindakan ini tidak dapat dibatalkan.",
            "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
        {
            try
            {
                using var db = new AppDbContext();
                var entity = await db.Orders.FindAsync(order.Id);
                if (entity != null)
                {
                    db.Orders.Remove(entity); // Cascade delete akan menghapus OrderItems juga
                    await db.SaveChangesAsync();
                }
                Orders.Remove(order);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menghapus: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}