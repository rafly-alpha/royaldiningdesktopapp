using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace RoyalDining.Desktop.ViewModels.Admin;

public partial class AdminDashboardViewModel : ObservableObject
{
    [ObservableProperty] private int totalCategories;
    [ObservableProperty] private int totalMenus;
    [ObservableProperty] private int totalOrders;
    [ObservableProperty] private int totalCustomers;
    [ObservableProperty] private string totalRevenue = "Rp 0";

    // Data untuk grafik: List jumlah pesanan per hari (7 hari terakhir)
    public ObservableCollection<int> ChartValues { get; } = new();
    public ObservableCollection<string> ChartLabels { get; } = new();

    // Property untuk LiveCharts
    public ISeries[] ChartSeries { get; private set; } = Array.Empty<ISeries>();
    public Axis[] ChartXAxes { get; private set; } = Array.Empty<Axis>();
    public Axis[] ChartYAxes { get; private set; } = Array.Empty<Axis>();

    public AdminDashboardViewModel()
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        using var db = new AppDbContext();

        // 1. Ambil metrik dari database
        TotalCategories = await db.Categories.CountAsync();
        TotalMenus = await db.Menus.CountAsync();
        TotalOrders = await db.Orders.CountAsync(o => o.Status != "Pending");
        TotalCustomers = await db.Users.CountAsync(u => u.Role == "Customer");

        var revenue = await db.Orders
            .Where(o => o.Status == "Completed")
            .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
        TotalRevenue = $"Rp {revenue:N0}";

        // 2. Ambil data grafik 7 hari terakhir
        ChartValues.Clear();
        ChartLabels.Clear();

        var today = DateTime.Today;
        var startDate = today.AddDays(-6);

        // Ambil semua order dalam 7 hari terakhir
        var orders = await db.Orders
            .Where(o => o.CreatedAt >= startDate && o.Status != "Pending")
            .ToListAsync();

        // Loop 7 hari ke belakang
        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            var count = orders.Count(o => o.CreatedAt.Date == date);

            ChartValues.Add(count);
            ChartLabels.Add(date.ToString("ddd, dd MMM")); // Contoh: "Mon, 20 Oct"
        }

        UpdateChart();
    }

    private void UpdateChart()
    {
        // Series (Line Chart)
        ChartSeries = new ISeries[]
        {
        new LineSeries<int>
        {
            Values = ChartValues,
            Fill = new SolidColorPaint(SKColor.Parse("#C9A227").WithAlpha(40)),
            Stroke = new SolidColorPaint(SKColor.Parse("#C9A227")) { StrokeThickness = 3 },
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint(SKColor.Parse("#C9A227")) { StrokeThickness = 3 },
            GeometryFill = new SolidColorPaint(SKColors.White),
            Name = "Pesanan"
        }
        };

        // X Axis (Labels)
        ChartXAxes = new Axis[]
        {
        new Axis
        {
            Labels = ChartLabels.ToArray(),
            LabelsRotation = 0,
            TextSize = 12,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(50))
        }
        };

        // Y Axis
        ChartYAxes = new Axis[]
        {
        new Axis
        {
            MinLimit = 0,
            TextSize = 12,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(50))
        }
        };

        // Notify UI
        OnPropertyChanged(nameof(ChartSeries));
        OnPropertyChanged(nameof(ChartXAxes));
        OnPropertyChanged(nameof(ChartYAxes));
    }
}