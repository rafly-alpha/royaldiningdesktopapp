using MaterialDesignThemes.Wpf;
using RoyalDining.Desktop.Models;
using RoyalDining.Desktop.ViewModels.Customer;
using System.Windows;
using System.Windows.Controls;

namespace RoyalDining.Desktop.Views.Customer;

public partial class HistoryPage : UserControl
{
    private readonly OrderHistoryViewModel _viewModel;

    public HistoryPage()
    {
        InitializeComponent();
        _viewModel = new OrderHistoryViewModel();
        DataContext = _viewModel;
        Loaded += async (s, e) => await _viewModel.LoadDataAsync();
    }

    private async void BtnDetail_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Order order)
        {
            var dialog = new CustomerOrderDetailDialog(order);
            await DialogHost.Show(dialog, "RootDialog");
        }
    }
}