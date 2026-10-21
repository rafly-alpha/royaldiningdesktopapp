using RoyalDining.Desktop.Models;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace RoyalDining.Desktop.Views.Customer;

public partial class CustomerOrderDetailDialog : UserControl
{
    public CustomerOrderDetailDialog(Order order)
    {
        InitializeComponent();

        TxtTitle.Text = $"Detail Pesanan #{order.Id}";
        TxtDate.Text = order.CreatedAt.ToString("dd MMM yyyy, HH:mm");
        TxtTotal.Text = $"Rp {order.TotalPrice:N0}";
        ItemsList.ItemsSource = order.OrderItems;

        TxtStatus.Text = order.Status;
        if (order.Status == "Completed")
        {
            StatusBorder.Background = System.Windows.Media.Brushes.LightGreen;
            TxtStatus.Foreground = System.Windows.Media.Brushes.Green;
        }
        else if (order.Status == "Cancelled")
        {
            StatusBorder.Background = System.Windows.Media.Brushes.LightPink;
            TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
        else if (order.Status == "Processing")
        {
            StatusBorder.Background = System.Windows.Media.Brushes.LightBlue;
            TxtStatus.Foreground = System.Windows.Media.Brushes.Blue;
        }
        else
        {
            StatusBorder.Background = System.Windows.Media.Brushes.LightYellow;
            TxtStatus.Foreground = System.Windows.Media.Brushes.Orange;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        DialogHost.CloseDialogCommand.Execute(null, null);
    }
}