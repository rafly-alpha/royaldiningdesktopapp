using MaterialDesignThemes.Wpf;
using RoyalDining.Desktop.Models;
using RoyalDining.Desktop.ViewModels.Admin;
using System.Windows;
using System.Windows.Controls;

namespace RoyalDining.Desktop.Views.Admin;

public partial class OrdersPage : UserControl
{
    private readonly OrdersViewModel _viewModel;

    public OrdersPage()
    {
        InitializeComponent();
        _viewModel = new OrdersViewModel();
        DataContext = _viewModel;
    }

    private async void BtnDetail_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Order order)
        {
            var dialog = new OrderDetailDialog(order);
            var result = await DialogHost.Show(dialog, "RootDialog");

            if (result is bool saved && saved)
                await _viewModel.LoadDataAsync();
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Order order)
        {
            await _viewModel.DeleteOrderCommand.ExecuteAsync(order);
        }
    }
}