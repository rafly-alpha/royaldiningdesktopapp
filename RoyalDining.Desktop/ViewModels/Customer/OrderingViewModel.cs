using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using RoyalDining.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace RoyalDining.Desktop.ViewModels.Customer;

// Class bantuan untuk mengelompokkan menu
public class MenuGroup : ObservableCollection<Menu>
{
    public string CategoryName { get; private set; }
    public string Description { get; private set; }

    public MenuGroup(string categoryName, string description, IEnumerable<Menu> menus) : base(menus)
    {
        CategoryName = categoryName;
        Description = description;
    }
}

public partial class OrderingViewModel : ObservableObject
{
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<MenuGroup> MenuGroups { get; } = new(); // Gunakan ini
    public ObservableCollection<OrderItem> CartItems { get; } = new();

    [ObservableProperty] private Order? activeOrder;
    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private decimal totalPrice;
    [ObservableProperty] private bool isBusy;

    private List<Menu> _allMenus = new();

    public OrderingViewModel()
    {
        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        using var db = new AppDbContext();

        // 1. Load Kategori untuk Dropdown Filter
        var cats = await db.Categories.OrderBy(c => c.Name).ToListAsync();
        Categories.Clear();
        Categories.Add(new Category { Id = 0, Name = "Semua Kategori" }); // Opsi All
        foreach (var c in cats) Categories.Add(c);

        SelectedCategory = Categories[0];

        // 2. Load Semua Menu (beserta Kategori)
        _allMenus = await db.Menus.Include(m => m.Category).OrderBy(m => m.Name).ToListAsync();

        // Panggil filter
        ApplyFilter();

        // 3. Cek atau Buat Order Pending
        var userId = SessionService.CurrentUser!.Id;
        var pending = await db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Menu)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "Pending");

        if (pending == null)
        {
            pending = new Order { UserId = userId, Status = "Pending", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
            db.Orders.Add(pending);
            await db.SaveChangesAsync();
        }

        ActiveOrder = pending;
        LoadCart();

        IsBusy = false;
    }

    partial void OnSelectedCategoryChanged(Category? value) => ApplyFilter();
    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        MenuGroups.Clear();
        var filtered = _allMenus.AsEnumerable();

        // Filter by Category (jika bukan "Semua Kategori")
        if (SelectedCategory != null && SelectedCategory.Id != 0)
            filtered = filtered.Where(m => m.CategoryId == SelectedCategory.Id);

        // Filter by Search Text
        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(m => m.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        // Kelompokkan berdasarkan Kategori
        var grouped = filtered
            .GroupBy(m => new { m.Category.Name, m.Category.Description })
            .OrderBy(g => g.Key.Name);

        foreach (var group in grouped)
        {
            MenuGroups.Add(new MenuGroup(
                group.Key.Name,
                group.Key.Description ?? "",
                group.ToList()
            ));
        }
    }

    private void LoadCart()
    {
        CartItems.Clear();
        if (ActiveOrder == null) return;

        foreach (var item in ActiveOrder.OrderItems)
            CartItems.Add(item);

        TotalPrice = CartItems.Sum(x => x.Subtotal);
    }

    [RelayCommand]
    private async Task AddToCartAsync(Menu menu)
    {
        if (ActiveOrder == null || menu == null) return;

        using var db = new AppDbContext();
        var orderId = ActiveOrder.Id;

        var existingItem = await db.OrderItems
            .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.MenuId == menu.Id);

        if (existingItem != null)
        {
            existingItem.Quantity++;
            existingItem.Subtotal = existingItem.Quantity * menu.Price;
            existingItem.UpdatedAt = DateTime.Now;
        }
        else
        {
            db.OrderItems.Add(new OrderItem
            {
                OrderId = orderId,
                MenuId = menu.Id,
                Quantity = 1,
                Subtotal = menu.Price,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        await db.SaveChangesAsync();
        await RefreshCartAsync();
    }

    [RelayCommand]
    private async Task DecreaseQuantityAsync(OrderItem item)
    {
        if (item == null || ActiveOrder == null) return;

        using var db = new AppDbContext();
        var entity = await db.OrderItems.FindAsync(item.Id);
        if (entity == null) return;

        if (entity.Quantity > 1)
        {
            entity.Quantity--;
            entity.Subtotal = entity.Quantity * item.Menu!.Price;
            entity.UpdatedAt = DateTime.Now;
        }
        else
        {
            db.OrderItems.Remove(entity);
        }

        await db.SaveChangesAsync();
        await RefreshCartAsync();
    }

    private async Task RefreshCartAsync()
    {
        if (ActiveOrder == null) return;
        using var db = new AppDbContext();
        var order = await db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Menu)
            .FirstOrDefaultAsync(o => o.Id == ActiveOrder.Id);

        ActiveOrder = order;
        LoadCart();
    }

    [RelayCommand]
    private async Task SubmitOrderAsync()
    {
        if (ActiveOrder == null || !CartItems.Any())
        {
            MessageBox.Show("Keranjang masih kosong.", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show("Yakin ingin submit pesanan ini? Anda tidak bisa mengubahnya setelah submit.",
            "Konfirmasi Submit", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            using var db = new AppDbContext();
            var order = await db.Orders.FindAsync(ActiveOrder.Id);
            if (order != null)
            {
                order.Status = "Waiting Payment";
                order.TotalPrice = TotalPrice;
                order.UpdatedAt = DateTime.Now;
                await db.SaveChangesAsync();
            }
            MessageBox.Show("Pesanan berhasil di-submit! Silakan bayar di kasir.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService.Navigate(new Views.Customer.MyOrdersPage());
        }
    }

    [RelayCommand]
    private async Task CancelOrderAsync()
    {
        if (ActiveOrder == null) return;

        var confirm = MessageBox.Show("Yakin ingin membatalkan pesanan ini? Semua item akan dihapus.",
            "Konfirmasi Batal", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
        {
            using var db = new AppDbContext();
            var order = await db.Orders.FindAsync(ActiveOrder.Id);
            if (order != null)
            {
                db.Orders.Remove(order);
                await db.SaveChangesAsync();
            }
            NavigationService.Navigate(new Views.Customer.CustomerDashboardPage());
        }
    }
}