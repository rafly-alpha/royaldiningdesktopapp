using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using MenuModel = RoyalDining.Desktop.Models.Menu; // Alias agar tidak bentrok

namespace RoyalDining.Desktop.Views.Admin;

public partial class MenuFormDialog : UserControl
{
    private readonly MenuModel? _existing;
    private string? _selectedImagePath;

    private static readonly string ImagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images");

    public MenuFormDialog(MenuModel? menu = null, List<Category>? categories = null)
    {
        InitializeComponent();
        _existing = menu;

        if (categories != null)
            CmbCategory.ItemsSource = categories;

        if (!Directory.Exists(ImagesFolder))
            Directory.CreateDirectory(ImagesFolder);

        if (menu != null)
        {
            TxtTitle.Text = "Edit Menu";
            TxtName.Text = menu.Name;
            TxtPrice.Text = menu.Price.ToString("F0");
            TxtDescription.Text = menu.Description;
            CmbCategory.SelectedValue = menu.CategoryId;
            _selectedImagePath = menu.ImagePath;

            if (!string.IsNullOrEmpty(menu.ImagePath))
            {
                LoadImagePreview(menu.ImagePath);
                TxtImageName.Text = menu.ImagePath;
            }
        }
    }

    private void BtnPickImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.bmp",
            Title = "Pilih Gambar Menu"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var sourceFile = dialog.FileName;
                var fileName = Path.GetFileName(sourceFile);
                var destFile = Path.Combine(ImagesFolder, fileName);
                File.Copy(sourceFile, destFile, overwrite: true);

                _selectedImagePath = fileName;
                TxtImageName.Text = fileName;
                LoadImagePreview(fileName);
            }
            catch (Exception ex)
            {
                ShowError($"Gagal memuat gambar: {ex.Message}");
            }
        }
    }

    private void LoadImagePreview(string fileName)
    {
        try
        {
            var fullPath = Path.Combine(ImagesFolder, fileName);
            if (File.Exists(fullPath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(fullPath);
                bitmap.EndInit();

                ImgPreview.Source = bitmap;
                ImgPreview.Visibility = Visibility.Visible;
                IconPlaceholder.Visibility = Visibility.Collapsed;
            }
        }
        catch { /* ignore */ }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtName.Text.Trim();
        var priceText = TxtPrice.Text.Trim();
        var description = TxtDescription.Text.Trim();
        var categoryId = CmbCategory.SelectedValue as int?;

        if (string.IsNullOrWhiteSpace(name)) { ShowError("Nama menu wajib diisi."); return; }
        if (categoryId == null) { ShowError("Kategori wajib dipilih."); return; }
        if (!decimal.TryParse(priceText, out var price) || price < 0) { ShowError("Harga harus berupa angka positif."); return; }

        try
        {
            using var db = new AppDbContext();

            var isDuplicate = await db.Menus.AnyAsync(m =>
                m.Name.ToLower() == name.ToLower() &&
                (_existing == null || m.Id != _existing.Id));

            if (isDuplicate) { ShowError($"Menu '{name}' sudah ada."); return; }

            if (_existing == null)
            {
                db.Menus.Add(new MenuModel
                {
                    Name = name,
                    Price = price,
                    Description = description,
                    CategoryId = categoryId.Value,
                    ImagePath = _selectedImagePath,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                var entity = await db.Menus.FindAsync(_existing.Id);
                if (entity != null)
                {
                    entity.Name = name; entity.Price = price; entity.Description = description;
                    entity.CategoryId = categoryId.Value; entity.ImagePath = _selectedImagePath;
                    entity.UpdatedAt = DateTime.Now;
                }
            }

            await db.SaveChangesAsync();
            DialogHost.CloseDialogCommand.Execute(true, null);
        }
        catch (Exception ex) { ShowError($"Gagal menyimpan: {ex.Message}"); }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogHost.CloseDialogCommand.Execute(false, null);
    private void ShowError(string message) { TxtError.Text = message; TxtError.Visibility = Visibility.Visible; }
}