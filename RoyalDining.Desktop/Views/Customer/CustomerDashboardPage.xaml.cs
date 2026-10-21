using RoyalDining.Desktop.Services;
using System.Windows;
using System.Windows.Controls;

namespace RoyalDining.Desktop.Views.Customer;

public partial class CustomerDashboardPage : UserControl
{
    public CustomerDashboardPage()
    {
        InitializeComponent();
    }

    private void BtnStartOrder_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Navigate(new OrderingPage());
    }
}