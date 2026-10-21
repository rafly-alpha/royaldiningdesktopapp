using MaterialDesignThemes.Wpf;
using RoyalDining.Desktop.Models;
using RoyalDining.Desktop.ViewModels.Admin;
using System.Windows;
using System.Windows.Controls;

namespace RoyalDining.Desktop.Views.Admin;

public partial class CategoriesPage : UserControl
{
    private readonly CategoriesViewModel _viewModel;

    public CategoriesPage()
    {
        InitializeComponent();
        _viewModel = new CategoriesViewModel();
        DataContext = _viewModel;
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CategoryFormDialog();
        var result = await DialogHost.Show(dialog, "RootDialog");

        if (result is bool saved && saved)
            await _viewModel.LoadDataAsync();
    }

    private async void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Category category)
        {
            var dialog = new CategoryFormDialog(category);
            var result = await DialogHost.Show(dialog, "RootDialog");

            if (result is bool saved && saved)
                await _viewModel.LoadDataAsync();
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Category category)
        {
            await _viewModel.DeleteCategoryCommand.ExecuteAsync(category);
        }
    }
}