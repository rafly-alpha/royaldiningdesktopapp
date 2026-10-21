using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace RoyalDining.Desktop.Views.Admin;

public partial class OrderDetailDialog : UserControl
{
    private readonly Order _order;

    public OrderDetailDialog(Order order)
    {
        InitializeComponent();
        _order = order;

        // Isi data
        TxtTitle.Text = $"Detail Pesanan #{order.Id}";
        TxtCustomer.Text = order.User?.Name ?? "Unknown";
        TxtDate.Text = order.CreatedAt.ToString("dd MMM yyyy, HH:mm");
        TxtTotal.Text = $"Rp {order.TotalPrice:N0}";

        // Isi item
        ItemsList.ItemsSource = order.OrderItems;

        // Set status di ComboBox
        foreach (ComboBoxItem item in CmbStatus.Items)
        {
            if (item.Content.ToString() == order.Status)
            {
                CmbStatus.SelectedItem = item;
                break;
            }
        }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (CmbStatus.SelectedItem is not ComboBoxItem selectedItem) return;
        var newStatus = selectedItem.Content.ToString();

        if (newStatus == _order.Status)
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var entity = await db.Orders.FindAsync(_order.Id);
            if (entity != null)
            {
                entity.Status = newStatus!;
                entity.UpdatedAt = DateTime.Now;
                await db.SaveChangesAsync();
            }

            DialogHost.CloseDialogCommand.Execute(true, null);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal menyimpan status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogHost.CloseDialogCommand.Execute(false, null);
    }
}