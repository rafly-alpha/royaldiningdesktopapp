using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using RoyalDining.Desktop.Services;
using System.Collections.ObjectModel;

namespace RoyalDining.Desktop.ViewModels.Customer;

public partial class OrderHistoryViewModel : ObservableObject
{
    public ObservableCollection<Order> Orders { get; } = new();
    public ObservableCollection<Order> ActiveOrders { get; } = new();

    [ObservableProperty] private bool isBusy;

    public OrderHistoryViewModel()
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsBusy = true;
        Orders.Clear();
        ActiveOrders.Clear();

        var userId = SessionService.CurrentUser!.Id;

        using var db = new AppDbContext();

        // Load History (Completed / Cancelled)
        var history = await db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Menu)
            .Where(o => o.UserId == userId && (o.Status == "Completed" || o.Status == "Cancelled"))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        foreach (var o in history) Orders.Add(o);

        // Load Active Orders (Waiting Payment / Processing)
        var active = await db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Menu)
            .Where(o => o.UserId == userId && (o.Status == "Waiting Payment" || o.Status == "Processing"))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        foreach (var o in active) ActiveOrders.Add(o);

        IsBusy = false;
    }
}