using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using RoyalDining.Desktop.Helpers;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace RoyalDining.Desktop.Views.Admin;

public partial class UserFormDialog : UserControl
{
    private readonly User? _existing;

    public UserFormDialog(User? user = null)
    {
        InitializeComponent();
        _existing = user;

        if (user != null)
        {
            TxtTitle.Text = "Edit User";
            TxtName.Text = user.Name;
            TxtEmail.Text = user.Email;
            TxtPasswordLabel.Text = "Password Baru (Opsional)";
            TxtPasswordHint.Visibility = Visibility.Visible;

            foreach (ComboBoxItem item in CmbRole.Items)
            {
                if (item.Content.ToString() == user.Role)
                {
                    CmbRole.SelectedItem = item;
                    break;
                }
            }
        }
        else
        {
            CmbRole.SelectedIndex = 0; // Default Customer
        }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtName.Text.Trim();
        var email = TxtEmail.Text.Trim();
        var password = TxtPassword.Password;
        var role = (CmbRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Customer";

        // Validasi
        if (string.IsNullOrWhiteSpace(name)) { ShowError("Nama wajib diisi."); return; }
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) { ShowError("Email tidak valid."); return; }

        // Validasi password
        if (_existing == null && string.IsNullOrWhiteSpace(password))
        {
            ShowError("Password wajib diisi untuk user baru.");
            return;
        }
        if (!string.IsNullOrWhiteSpace(password) && password.Length < 6)
        {
            ShowError("Password minimal 6 karakter.");
            return;
        }

        try
        {
            using var db = new AppDbContext();

            // Cek duplikat email
            var isDuplicate = await db.Users.AnyAsync(u =>
                u.Email.ToLower() == email.ToLower() &&
                (_existing == null || u.Id != _existing.Id));

            if (isDuplicate) { ShowError($"Email '{email}' sudah terdaftar."); return; }

            if (_existing == null)
            {
                // Tambah baru
                db.Users.Add(new User
                {
                    Name = name,
                    Email = email.ToLower(),
                    PasswordHash = PasswordHasher.Hash(password),
                    Role = role,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                // Edit
                var entity = await db.Users.FindAsync(_existing.Id);
                if (entity != null)
                {
                    entity.Name = name;
                    entity.Email = email.ToLower();
                    entity.Role = role;
                    if (!string.IsNullOrWhiteSpace(password))
                        entity.PasswordHash = PasswordHasher.Hash(password);
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