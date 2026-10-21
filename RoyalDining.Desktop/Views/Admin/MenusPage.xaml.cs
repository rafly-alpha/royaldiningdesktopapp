using MaterialDesignThemes.Wpf;
using RoyalDining.Desktop.ViewModels.Admin;
using System.Windows;
using System.Windows.Controls;
using MenuModel = RoyalDining.Desktop.Models.Menu; // Alias

namespace RoyalDining.Desktop.Views.Admin;

public partial class MenusPage : UserControl
{
    private readonly MenusViewModel _viewModel;

    public MenusPage()
    {
        InitializeComponent();
        _viewModel = new MenusViewModel();
        DataContext = _viewModel;
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new MenuFormDialog(categories: _viewModel.Categories.ToList());
        var result = await DialogHost.Show(dialog, "RootDialog");
        if (result is bool saved && saved) await _viewModel.LoadDataAsync();
    }

    private async void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is MenuModel menu)
        {
            var dialog = new MenuFormDialog(menu, _viewModel.Categories.ToList());
            var result = await DialogHost.Show(dialog, "RootDialog");
            if (result is bool saved && saved) await _viewModel.LoadDataAsync();
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is MenuModel menu)
        {
            await _viewModel.DeleteMenuCommand.ExecuteAsync(menu);
        }
    }
}