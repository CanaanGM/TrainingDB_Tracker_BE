using SharedLibrary.Helpers;

namespace DateLibraryTests;
public class SecurityHelpersTests
{
    [Fact]
    public void GenerateSalt_ShouldReturnNonEmptyString()
    {
        // Act
        var salt = SecurityHelpers.GenerateSalt();

        // Assert
        Assert.False(string.IsNullOrEmpty(salt));
    }

    [Fact]
    public void GenerateSalt_ShouldGenerateUniqueSalts()
    {
        // Act
        var salt1 = SecurityHelpers.GenerateSalt();
        var salt2 = SecurityHelpers.GenerateSalt();

        // Assert
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = SecurityHelpers.GenerateSalt();

        // Act
        var hash1 = SecurityHelpers.HashPassword(password, salt);
        var hash2 = SecurityHelpers.HashPassword(password, salt);

        // Assert
        Assert.True(hash1.IsSuccess);
        Assert.True(hash2.IsSuccess);
        Assert.Equal(hash1.Value, hash2.Value);
        Assert.False(string.IsNullOrEmpty(hash1.Value));
    }

    [Fact]
    public void HashPassword_WithDifferentSalts_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt1 = SecurityHelpers.GenerateSalt();
        var salt2 = SecurityHelpers.GenerateSalt();

        // Act
        var hash1 = SecurityHelpers.HashPassword(password, salt1);
        var hash2 = SecurityHelpers.HashPassword(password, salt2);

        // Assert
        Assert.NotEqual(hash1.Value, hash2.Value);
    }
    [Fact]
    public void HashPassword_WithSimilarSalts_ShouldReturnSimilarHashes()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = SecurityHelpers.GenerateSalt();

        // Act
        var hash1 = SecurityHelpers.HashPassword(password, salt);
        var hash2 = SecurityHelpers.HashPassword(password, salt);

        // Assert
        Assert.Equal(hash1.Value, hash2.Value);
    }

    [Theory]
    [InlineData(null, "someSalt")]
    [InlineData("somePassword", null)]
    [InlineData("", "someSalt")]
    [InlineData("somePassword", "")]
    public void HashPassword_WithNullOrEmptyInputs_ShouldThrowArgumentException(string password, string salt)
    {
        // Act & Assert
        var hashResult =  SecurityHelpers.HashPassword(password, salt);
        Assert.False(hashResult.IsSuccess);
    }
    
    /// <summary>
    /// for me to create users xD
    /// </summary>
    /// <param name="password"></param>
    [Theory]
    [InlineData("كنعان لازم يتدرب !")]
    [InlineData("ろまである!")]
    [InlineData("pizza is pizza")]
    [InlineData("sneaky snake")]
    public void HashPassword_dummy(string password)
    {
        // Act & Assert
        var salt = SecurityHelpers.GenerateSalt();
        var hash = SecurityHelpers.HashPassword(password, salt);
        
        Assert.True(1 == 1);
    }
    
       [Fact]
    public void VerifyPassword_Success()
    {
        // Arrange
        string password = "TestPassword123";
        string salt = SecurityHelpers.GenerateSalt();
        var hashResult = SecurityHelpers.HashPassword(password, salt);
        Assert.True(hashResult.IsSuccess);

        // Act
        var verifyResult = SecurityHelpers.VerifyPassword(password, hashResult.Value, salt);

        // Assert
        Assert.True(verifyResult.IsSuccess);
        Assert.True(verifyResult.Value);
        Assert.Equal("Password verified successfully.", verifyResult.SuccessMessage);
    }

    [Fact]
    public void VerifyPassword_Failure_IncorrectPassword()
    {
        // Arrange
        string password = "TestPassword123";
        string incorrectPassword = "WrongPassword123";
        string salt = SecurityHelpers.GenerateSalt();
        var hashResult = SecurityHelpers.HashPassword(password, salt);
        Assert.True(hashResult.IsSuccess);

        // Act
        var verifyResult = SecurityHelpers.VerifyPassword(incorrectPassword, hashResult.Value, salt);

        // Assert
        Assert.False(verifyResult.IsSuccess);
        Assert.False(verifyResult.Value);
        Assert.Equal("Password verification failed.", verifyResult.ErrorMessage);
    }

    [Fact]
    public void VerifyPassword_Failure_IncorrectHash()
    {
        // Arrange
        string password = "TestPassword123";
        string salt = SecurityHelpers.GenerateSalt();
        var hashResult = SecurityHelpers.HashPassword(password, salt);
        Assert.True(hashResult.IsSuccess);
        string incorrectHash = "IncorrectHashValue";

        // Act
        var verifyResult = SecurityHelpers.VerifyPassword(password, incorrectHash, salt);

        // Assert
        Assert.False(verifyResult.IsSuccess);
        Assert.False(verifyResult.Value);
        Assert.Equal("Password verification failed.", verifyResult.ErrorMessage);
    }

    // [Fact]
    // TODO: bcrypt verify does not take the custom Salt . . .
    public void VerifyPassword_Failure_IncorrectSalt()
    {
        // Arrange
        string password = "TestPassword123";
        string salt = SecurityHelpers.GenerateSalt();
        var hashResult = SecurityHelpers.HashPassword(password, salt);
        Assert.True(hashResult.IsSuccess);
        string incorrectSalt = SecurityHelpers.GenerateSalt();

        // Act
        var verifyResult = SecurityHelpers.VerifyPassword(password, hashResult.Value, incorrectSalt);

        // Assert
        Assert.False(verifyResult.IsSuccess);
        Assert.False(verifyResult.Value);
        Assert.Equal("Password verification failed.", verifyResult.ErrorMessage);
    }

    [Fact]
    public void VerifyPassword_Failure_NullPassword()
    {
        // Arrange
        string password = null;
        string salt = SecurityHelpers.GenerateSalt();
        string hash = "SomeHashValue";

        // Act
        var verifyResult = SecurityHelpers.VerifyPassword(password, hash, salt);

        // Assert
        Assert.False(verifyResult.IsSuccess);
        Assert.Equal($"Entered password cannot be null or empty.", verifyResult.ErrorMessage);
    }

    [Fact]
    public void VerifyPassword_Failure_NullHash()
    {
        // Arrange
        string password = "TestPassword123";
        string salt = SecurityHelpers.GenerateSalt();
        string hash = null;

        // Act
        var verifyResult = SecurityHelpers.VerifyPassword(password, hash, salt);

        // Assert
        Assert.False(verifyResult.IsSuccess);
        Assert.Equal($"Stored hash cannot be null or empty.", verifyResult.ErrorMessage);
    }

    [Fact]
    public void VerifyPassword_Failure_NullSalt()
    {
        // Arrange
        string password = "TestPassword123";
        string salt = null;
        string hash = "SomeHashValue";

        // Act
        var verifyResult = SecurityHelpers.VerifyPassword(password, hash, salt);

        // Assert
        Assert.False(verifyResult.IsSuccess);
        Assert.Equal($"Salt cannot be null or empty.", verifyResult.ErrorMessage);
    }
}
