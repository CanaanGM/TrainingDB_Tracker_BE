using SharedLibrary.Core;

namespace SharedLibrary.Helpers;

public static class SecurityHelpers
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
        try
        {
            if (string.IsNullOrEmpty(password))
                return Result<string>.Failure($"Password cannot be null or empty: {nameof(password)}.");
            if (string.IsNullOrEmpty(salt))
                return Result<string>.Failure($"Salt cannot be null or empty: {nameof(salt)}.");

            return Result<string>.Success(BCrypt.Net.BCrypt.HashPassword(inputKey: password, salt: salt),
                "generated hash successfully");

        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Generates a random salt using BCrypt.
    /// </summary>
    /// <returns>salt using BCryp.</returns>
    public static string GenerateSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt();
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
    public static Result<bool> VerifyPassword(string enteredPassword, string storedHash, string salt = "not used")//not needed the salt
    {
        if (string.IsNullOrEmpty(enteredPassword))
            return Result<bool>.Failure($"Entered password cannot be null or empty.");
        if (string.IsNullOrEmpty(storedHash))
            return Result<bool>.Failure($"Stored hash cannot be null or empty.");
        if (string.IsNullOrEmpty(salt))
            return Result<bool>.Failure($"Salt cannot be null or empty.");

        try
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash)
                ? Result<bool>.Success(true, "Password verified successfully.")
                : Result<bool>.Failure("Password verification failed.");
        }
        catch (Exception e)
        {
            return Result<bool>.Failure("Password verification failed.");
        }
    }
}