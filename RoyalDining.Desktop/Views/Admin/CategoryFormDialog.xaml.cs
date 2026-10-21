using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace RoyalDining.Desktop.Views.Admin;

public partial class CategoryFormDialog : UserControl
{
    private readonly Category? _existing;

    public CategoryFormDialog(Category? category = null)
    {
        InitializeComponent();
        _existing = category;

        if (category != null)
        {
            TxtTitle.Text = "Edit Kategori";
            TxtName.Text = category.Name;
            TxtDescription.Text = category.Description;
        }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtName.Text.Trim();
        var description = TxtDescription.Text.Trim();

        // Validasi
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("Nama kategori wajib diisi.");
            return;
        }

        try
        {
            using var db = new AppDbContext();

            // Cek duplikat nama (kecuali dirinya sendiri saat edit)
            var isDuplicate = await db.Categories.AnyAsync(c =>
                c.Name.ToLower() == name.ToLower() &&
                (_existing == null || c.Id != _existing.Id));

            if (isDuplicate)
            {
                ShowError($"Kategori '{name}' sudah ada.");
                return;
            }

            if (_existing == null)
            {
                // Tambah baru
                db.Categories.Add(new Category
                {
                    Name = name,
                    Description = description,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                // Edit
                var entity = await db.Categories.FindAsync(_existing.Id);
                if (entity != null)
                {
                    entity.Name = name;
                    entity.Description = description;
                    entity.UpdatedAt = DateTime.Now;
                }
            }

            await db.SaveChangesAsync();
            DialogHost.CloseDialogCommand.Execute(true, null);
        }
        catch (Exception ex)
        {
            ShowError($"Gagal menyimpan: {ex.Message}");
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogHost.CloseDialogCommand.Execute(false, null);
    }

    private void ShowError(string message)
    {
        TxtError.Text = message;
        TxtError.Visibility = Visibility.Visible;
    }
}