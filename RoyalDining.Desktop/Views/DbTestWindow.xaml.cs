using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Helpers;
using System.Text;
using System.Windows;

namespace RoyalDining.Desktop.Views;

public partial class DbTestWindow : Window
{
    public DbTestWindow()
    {
        InitializeComponent();
    }

    private async void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        BtnTest.IsEnabled = false;
        TxtResult.Text = "⏳ Testing...";
        var sb = new StringBuilder();

        try
        {
            using var db = new AppDbContext();

            // ============ 1. TEST KONEKSI ============
            sb.AppendLine("=== 1. TEST KONEKSI ===");
            var canConnect = await db.Database.CanConnectAsync();
            if (canConnect)
                sb.AppendLine("✅ Koneksi ke database BERHASIL");
            else
            {
                sb.AppendLine("❌ Koneksi GAGAL");
                TxtResult.Text = sb.ToString();
                BtnTest.IsEnabled = true;
                return;
            }
            sb.AppendLine();

            // ============ 2. TEST USERS ============
            sb.AppendLine("=== 2. TEST USERS ===");
            var userCount = await db.Users.CountAsync();
            sb.AppendLine($"✅ Total Users: {userCount}");

            var admin = await db.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            if (admin != null)
                sb.AppendLine($"✅ Admin ditemukan: {admin.Name} ({admin.Email})");
            else
                sb.AppendLine("⚠️ Tidak ada admin ditemukan");
            sb.AppendLine();

            // ============ 3. TEST CATEGORIES & MENUS ============
            sb.AppendLine("=== 3. TEST CATEGORIES & MENUS ===");
            var catCount = await db.Categories.CountAsync();
            var menuCount = await db.Menus.CountAsync();
            sb.AppendLine($"✅ Total Categories: {catCount}");
            sb.AppendLine($"✅ Total Menus: {menuCount}");
            sb.AppendLine();

            // ============ 4. TEST ORDERS ============
            sb.AppendLine("=== 4. TEST ORDERS ===");
            var orderCount = await db.Orders.CountAsync();
            var itemCount = await db.OrderItems.CountAsync();
            sb.AppendLine($"✅ Total Orders: {orderCount}");
            sb.AppendLine($"✅ Total OrderItems: {itemCount}");
            sb.AppendLine();

            // ============ 5. TEST PASSWORD HASH ============
            sb.AppendLine("=== 5. TEST PASSWORD HASH ===");
            if (admin != null)
            {
                var isValid = PasswordHasher.Verify("royal123", admin.PasswordHash);
                if (isValid)
                    sb.AppendLine("✅ Password 'royal123' COCOK dengan hash admin");
                else
                    sb.AppendLine("❌ Password 'royal123' TIDAK cocok — hash di DB mungkin dummy");
            }
            sb.AppendLine();

            // ============ 6. TEST DASHBOARD METRICS ============
            sb.AppendLine("=== 6. DASHBOARD METRICS ===");
            var revenue = await db.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
            sb.AppendLine($"✅ Total Revenue: Rp {revenue:N0}");
            sb.AppendLine();

            sb.AppendLine("🎉 SEMUA TEST SELESAI!");

            // ============ 7. REGENERATE PASSWORD HASH ============
            //sb.AppendLine("=== 7. REGENERATE PASSWORD HASH ===");
            //var newHash = PasswordHasher.Hash("royal123");
            //sb.AppendLine($"Hash baru: {newHash}");
            //sb.AppendLine("⚠️ Copy hash ini ke SQL UPDATE di bawah:");
            //sb.AppendLine($"UPDATE Users SET PasswordHash = '{newHash}' WHERE Email = 'admin@webpro.com';");

            TxtResult.Text = sb.ToString();
        }
        catch (Exception ex)
        {
            sb.AppendLine();
            sb.AppendLine("❌ ERROR:");
            sb.AppendLine(ex.Message);
            sb.AppendLine();
            sb.AppendLine("Detail:");
            sb.AppendLine(ex.StackTrace);
            TxtResult.Text = sb.ToString();
        }
        finally
        {
            BtnTest.IsEnabled = true;
        }
    }
}