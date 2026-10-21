namespace RoyalDining.Desktop.Helpers;

public static class PasswordHasher
{
    private const int WorkFactor = 11;

    /// <summary>Hash password dengan BCrypt.</summary>
    public static string Hash(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new ArgumentException("Password tidak boleh kosong", nameof(plainPassword));

        return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
    }

    /// <summary>Verifikasi password dengan hash-nya.</summary>
    public static bool Verify(string plainPassword, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
}