using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace RoyalDining.Desktop.ViewModels.Admin;

public partial class ReportsViewModel : ObservableObject
{
    // Metrics
    [ObservableProperty] private string totalRevenue = "Rp 0";
    [ObservableProperty] private int totalOrders;
    [ObservableProperty] private int newCustomers;
    [ObservableProperty] private string averageOrderValue = "Rp 0";

    // Charts Data
    public ISeries[] RevenueSeries { get; private set; } = Array.Empty<ISeries>();
    public Axis[] RevenueXAxes { get; private set; } = Array.Empty<Axis>();
    public Axis[] RevenueYAxes { get; private set; } = Array.Empty<Axis>();

    public ISeries[] CategorySeries { get; private set; } = Array.Empty<ISeries>();

    // Tables
    public ObservableCollection<TopMenuDto> TopMenus { get; } = new();

    public class CategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
    }

    public ObservableCollection<Order> RecentOrders { get; } = new();

    public ReportsViewModel()
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        using var db = new AppDbContext();

        // =============================================
        // 1. KEY METRICS (Hanya Completed untuk Revenue)
        // =============================================
        var revenue = await db.Orders
            .Where(o => o.Status == "Completed")
            .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

        var totalOrders = await db.Orders
            .CountAsync(o => o.Status != "Pending"); // Total pesanan (termasuk proses)

        var newCustomers = await db.Users
            .CountAsync(u => u.Role == "Customer" && u.CreatedAt >= DateTime.Now.AddDays(-30));

        // AOV = Revenue / Jumlah pesanan yang Completed
        var completedOrdersCount = await db.Orders.CountAsync(o => o.Status == "Completed");
        var aov = completedOrdersCount > 0 ? revenue / completedOrdersCount : 0;

        TotalRevenue = $"Rp {revenue:N0}";
        TotalOrders = totalOrders;
        NewCustomers = newCustomers;
        AverageOrderValue = $"Rp {aov:N0}";

        // =============================================
        // 2. REVENUE TREND (30 Hari Terakhir, Completed)
        // =============================================
        var startDate = DateTime.Today.AddDays(-30);
        var dailyRevenue = await db.Orders
            .Where(o => o.Status == "Completed" && o.CreatedAt >= startDate)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.TotalPrice) })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var revValues = new List<double>();
        var revLabels = new List<string>();

        for (int i = 0; i < 30; i++)
        {
            var date = startDate.AddDays(i);
            var val = dailyRevenue.FirstOrDefault(x => x.Date == date)?.Total ?? 0;
            revValues.Add((double)val);
            revLabels.Add(date.ToString("dd MMM"));
        }

        UpdateRevenueChart(revValues, revLabels);

        // =============================================
        // 3. DISTRIBUSI KATEGORI (Completed)
        // =============================================
        var catDistribution = await db.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Menu)
            .ThenInclude(m => m.Category)
            .Where(oi => oi.Order.Status == "Completed") // Hanya Completed
            .GroupBy(oi => oi.Menu.Category.Name)
            .Select(g => new CategoryDto
            {
                CategoryName = g.Key,
                TotalRevenue = g.Sum(x => x.Subtotal)
            })
            .ToListAsync();

        UpdateCategoryChart(catDistribution);

        // =============================================
        // 4. TOP MENU (Completed)
        // =============================================
        var top = await db.OrderItems
            .Include(oi => oi.Menu)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.Status == "Completed")
            .GroupBy(oi => new { oi.Menu.Name, CategoryName = oi.Menu.Category.Name })
            .Select(g => new TopMenuDto
            {
                Name = g.Key.Name,
                Category = g.Key.CategoryName,
                QtySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Subtotal)
            })
            .OrderByDescending(x => x.QtySold)
            .Take(5)
            .ToListAsync();

        TopMenus.Clear();
        foreach (var t in top) TopMenus.Add(t);

        // =============================================
        // 5. RECENT ORDERS (Selain Pending)
        // =============================================
        var recent = await db.Orders
            .Include(o => o.User)
            .Where(o => o.Status != "Pending") // Jangan tampilkan keranjang
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToListAsync();

        RecentOrders.Clear();
        foreach (var r in recent) RecentOrders.Add(r);
    }

    private void UpdateRevenueChart(List<double> values, List<string> labels)
    {
        RevenueSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values,
                Fill = new SolidColorPaint(SKColor.Parse("#C9A227").WithAlpha(40)),
                Stroke = new SolidColorPaint(SKColor.Parse("#C9A227")) { StrokeThickness = 3 },
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#C9A227")) { StrokeThickness = 3 },
                GeometryFill = new SolidColorPaint(SKColors.White),
                Name = "Pendapatan"
            }
        };

        RevenueXAxes = new Axis[]
        {
            new Axis { Labels = labels, TextSize = 10, SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(30)) }
        };

        RevenueYAxes = new Axis[]
        {
            new Axis { Labeler = value => $"Rp {value:N0}", TextSize = 10, SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(30)) }
        };

        OnPropertyChanged(nameof(RevenueSeries));
        OnPropertyChanged(nameof(RevenueXAxes));
        OnPropertyChanged(nameof(RevenueYAxes));
    }

    private void UpdateCategoryChart(List<CategoryDto> data)
    {
        var series = new List<ISeries>();
        var colors = new[] { "#C9A227", "#2196F3", "#4CAF50", "#F44336", "#9C27B0", "#FF9800" };

        int i = 0;
        foreach (var item in data)
        {
            series.Add(new PieSeries<double>
            {
                Values = new double[] { (double)item.TotalRevenue },
                Name = item.CategoryName,
                Fill = new SolidColorPaint(SKColor.Parse(colors[i % colors.Length]))
            });
            i++;
        }

        CategorySeries = series.ToArray();
        OnPropertyChanged(nameof(CategorySeries));
    }
}

public class TopMenuDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int QtySold { get; set; }
    public decimal Revenue { get; set; }
}