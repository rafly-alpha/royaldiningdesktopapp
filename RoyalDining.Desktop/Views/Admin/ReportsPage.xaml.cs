using RoyalDining.Desktop.ViewModels.Admin;
using System.Windows.Controls;

namespace RoyalDining.Desktop.Views.Admin;

public partial class ReportsPage : UserControl
{
    public ReportsPage()
    {
        InitializeComponent();
        DataContext = new ReportsViewModel();
    }
}