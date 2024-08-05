using System.Security.Cryptography;
using System.Text;
using DataLibrary.Core;

public class SecurityHelper
{
    /// <summary>
    /// Hashes the given password using the specified salt.
    /// </summary>
    /// <param name="password">The password to be hashed.</param>
    /// <param name="salt">The salt to be used for hashing.</param>
    /// <returns>
    /// A Result object containing the hashed password as a base64 string and a message indicating success or failure.
    /// </returns>
    public static Result<string> HashPassword(string password, string salt)
    {
        if (string.IsNullOrEmpty(password))
           return Result<string>.Failure($"Password cannot be null or empty: {nameof(password)}." );
        if (string.IsNullOrEmpty(salt))
            return Result<string>.Failure($"Salt cannot be null or empty: {nameof(salt)}.");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashedBytes = hmac.ComputeHash(passwordBytes);
        return Result<string>.Success(Convert.ToBase64String(hashedBytes), "generated hash successfully");  
    }

    /// <summary>
    /// Generates a random salt.
    /// </summary>
    /// <returns>A unique salt as a base64 string.</returns>
    public static string GenerateSalt()
    {
        using var randomNumberGenerator =  RandomNumberGenerator.Create();
        var saltBytes = new byte[32]; // 256 bits
        randomNumberGenerator.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
    
    /// <summary>
    /// Verifies the entered password against the stored hash using the provided salt.
    /// </summary>
    /// <param name="enteredPassword">The password entered by the user.</param>
    /// <param name="storedHash">The hash of the password that was stored.</param>
    /// <param name="salt">The salt used to hash the password.</param>
    /// <returns>
    /// A Result object containing a boolean value indicating whether the password verification succeeded,
    /// and a message indicating success or failure.
    /// </returns>
    public static Result<bool> VerifyPassword(string enteredPassword, string storedHash, string salt)
    {
        if (string.IsNullOrEmpty(enteredPassword))
            return Result<bool>.Failure($"Entered password cannot be null or empty.");
        if (string.IsNullOrEmpty(storedHash))
            return Result<bool>.Failure($"Stored hash cannot be null or empty.");
        if (string.IsNullOrEmpty(salt))
            return Result<bool>.Failure($"Salt cannot be null or empty.");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
        var enteredPasswordBytes = Encoding.UTF8.GetBytes(enteredPassword);
        var enteredHashedBytes = hmac.ComputeHash(enteredPasswordBytes);
        var enteredHash = Convert.ToBase64String(enteredHashedBytes);

        return enteredHash == storedHash 
            ? Result<bool>.Success(true, "Password verified successfully.") 
            : Result<bool>.Failure("Password verification failed.");
    }

}