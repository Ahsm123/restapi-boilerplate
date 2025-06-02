using Isopoh.Cryptography.Argon2;

namespace Api.Services;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return Argon2.Hash(password);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            return Argon2.Verify(hash, password);
        }
        catch
        {
            return false;
        }
    }
}
