using RoyalDining.Desktop.ViewModels.Admin;
using System.Windows.Controls;
using System.Windows;

namespace RoyalDining.Desktop.Views.Admin;

public partial class AdminDashboardPage : UserControl
{
    public AdminDashboardPage()
    {
        InitializeComponent();
        DataContext = new AdminDashboardViewModel();
    }

    private void BtnGoCategories_Click(object sender, RoutedEventArgs e)
    {
        Services.NavigationService.Navigate(new CategoriesPage());
    }

    private void BtnGoMenus_Click(object sender, RoutedEventArgs e)
    {
        Services.NavigationService.Navigate(new MenusPage());
    }

    private void BtnGoOrders_Click(object sender, RoutedEventArgs e)
    {
        Services.NavigationService.Navigate(new OrdersPage());
    }

    private void BtnGoReports_Click(object sender, RoutedEventArgs e)
    {
        Services.NavigationService.Navigate(new ReportsPage());
    }
}