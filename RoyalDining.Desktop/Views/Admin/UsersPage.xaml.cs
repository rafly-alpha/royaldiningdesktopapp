using MaterialDesignThemes.Wpf;
using RoyalDining.Desktop.Models;
using RoyalDining.Desktop.ViewModels.Admin;
using System.Windows;
using System.Windows.Controls;

namespace RoyalDining.Desktop.Views.Admin;

public partial class UsersPage : UserControl
{
    private readonly UsersViewModel _viewModel;

    public UsersPage()
    {
        InitializeComponent();
        _viewModel = new UsersViewModel();
        DataContext = _viewModel;
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new UserFormDialog();
        var result = await DialogHost.Show(dialog, "RootDialog");
        if (result is bool saved && saved) await _viewModel.LoadDataAsync();
    }

    private async void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is User user)
        {
            var dialog = new UserFormDialog(user);
            var result = await DialogHost.Show(dialog, "RootDialog");
            if (result is bool saved && saved) await _viewModel.LoadDataAsync();
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is User user)
        {
            await _viewModel.DeleteUserCommand.ExecuteAsync(user);
        }
    }
}