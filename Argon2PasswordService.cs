using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

public class Argon2PasswordService
{
    public static string HashPassword(string password)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            // Generate a salt
            var salt = new byte[16];
            rng.GetBytes(salt);

            // Create Argon2 hasher
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8;  // Threads used
            argon2.MemorySize = 65536;       // 64 MB memory used
            argon2.Iterations = 4;           // Number of iterations

            var hash = argon2.GetBytes(32);  // Hash size

            // Return the salt and hash as a Base64-encoded string
            var hashBytes = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, hashBytes, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, hashBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var hashBytes = Convert.FromBase64String(storedHash);

        // Extract the salt from the stored hash
        var salt = new byte[16];
        Buffer.BlockCopy(hashBytes, 0, salt, 0, salt.Length);

        // Hash the input password using the extracted salt
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = salt;
        argon2.DegreeOfParallelism = 8;
        argon2.MemorySize = 65536;
        argon2.Iterations = 4;

        var hash = argon2.GetBytes(32);

        // Compare the hashes
        for (int i = 0; i < hash.Length; i++)
        {
            if (hashBytes[i + salt.Length] != hash[i])
                return false;
        }

        return true;
    }
}
