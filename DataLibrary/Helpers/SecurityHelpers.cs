using System.Security.Cryptography;
using System.Text;

namespace DataLibrary.Helpers;
public class SecurityHelper
{
    /// <summary>
    /// Generates a hashed password using HMACSHA256.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="salt">The salt to apply to the hash.</param>
    /// <returns>A hashed password as a base64 string.</returns>
    public static string HashPassword(string password, string salt)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        
        if (string.IsNullOrEmpty(salt))
            throw new ArgumentException("Salt cannot be null or empty.", nameof(salt));

        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt)))
        {
            // Convert the password string to a byte array
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            // Compute the hash
            var hashedBytes = hmac.ComputeHash(passwordBytes);
            // Convert the hash to a Base64 string to make it easy to store
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// Generates a random salt.
    /// </summary>
    /// <returns>A unique salt as a base64 string.</returns>
    public static string GenerateSalt()
    {
        using (var randomNumberGenerator = new RNGCryptoServiceProvider())
        {
            var saltBytes = new byte[32]; // 256 bits
            randomNumberGenerator.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }
    }
}