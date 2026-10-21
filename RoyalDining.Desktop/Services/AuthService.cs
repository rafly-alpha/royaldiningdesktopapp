using Microsoft.EntityFrameworkCore;
using RoyalDining.Desktop.Data;
using RoyalDining.Desktop.Helpers;
using RoyalDining.Desktop.Models;

namespace RoyalDining.Desktop.Services;

public class AuthService
{
    /// <summary>
    /// Coba login. Return User jika berhasil, null jika gagal.
    /// </summary>
    public static async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Email dan password wajib diisi.", null);

        try
        {
            using var db = new AppDbContext();

            // Cari user berdasarkan email
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower().Trim());

            if (user == null)
                return (false, "Email tidak terdaftar.", null);

            // Verifikasi password
            if (!PasswordHasher.Verify(password, user.PasswordHash))
                return (false, "Password salah.", null);

            // Login berhasil
            SessionService.Login(user);
            return (true, "Login berhasil.", user);
        }
        catch (Exception ex)
        {
            return (false, $"Terjadi kesalahan sistem: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Register customer baru.
    /// </summary>
    public static async Task<(bool Success, string Message)> RegisterAsync(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Semua kolom wajib diisi.");

        if (password.Length < 6)
            return (false, "Password minimal 6 karakter.");

        if (!email.Contains("@") || !email.Contains("."))
            return (false, "Format email tidak valid.");

        try
        {
            using var db = new AppDbContext();

            // Cek email sudah terdaftar
            var existing = await db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower().Trim());
            if (existing)
                return (false, "Email sudah terdaftar. Silakan login.");

            // Buat user baru
            var newUser = new User
            {
                Name = name.Trim(),
                Email = email.ToLower().Trim(),
                PasswordHash = PasswordHasher.Hash(password),
                Role = "Customer",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return (true, "Registrasi berhasil. Silakan login.");
        }
        catch (Exception ex)
        {
            return (false, $"Terjadi kesalahan sistem: {ex.Message}");
        }
    }
}