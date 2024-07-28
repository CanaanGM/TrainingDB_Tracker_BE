using DataLibrary.Helpers;

namespace DateLibraryTests;

using Xunit;
using System;

public class SecurityHelperTests
{
    [Fact]
    public void GenerateSalt_ShouldReturnNonEmptyString()
    {
        // Act
        var salt = SecurityHelper.GenerateSalt();

        // Assert
        Assert.False(string.IsNullOrEmpty(salt));
    }

    [Fact]
    public void GenerateSalt_ShouldGenerateUniqueSalts()
    {
        // Act
        var salt1 = SecurityHelper.GenerateSalt();
        var salt2 = SecurityHelper.GenerateSalt();

        // Assert
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = SecurityHelper.GenerateSalt();

        // Act
        var hash1 = SecurityHelper.HashPassword(password, salt);
        var hash2 = SecurityHelper.HashPassword(password, salt);

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.False(string.IsNullOrEmpty(hash1));
    }

    [Fact]
    public void HashPassword_WithDifferentSalts_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt1 = SecurityHelper.GenerateSalt();
        var salt2 = SecurityHelper.GenerateSalt();

        // Act
        var hash1 = SecurityHelper.HashPassword(password, salt1);
        var hash2 = SecurityHelper.HashPassword(password, salt2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData(null, "someSalt")]
    [InlineData("somePassword", null)]
    [InlineData("", "someSalt")]
    [InlineData("somePassword", "")]
    public void HashPassword_WithNullOrEmptyInputs_ShouldThrowArgumentException(string password, string salt)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => SecurityHelper.HashPassword(password, salt));
        Assert.Contains("cannot be null or empty", ex.Message);
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
        var salt = SecurityHelper.GenerateSalt();
        var hash = SecurityHelper.HashPassword(password, salt);
        
        Assert.True(1 == 1);
    }
}
