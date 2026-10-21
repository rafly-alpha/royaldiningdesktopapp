using RoyalDining.Desktop.ViewModels.Customer;
using System.Windows.Controls;

namespace RoyalDining.Desktop.Views.Customer;

public partial class OrderingPage : UserControl
{
    public OrderingPage()
    {
        InitializeComponent();
        DataContext = new OrderingViewModel();
    }
}