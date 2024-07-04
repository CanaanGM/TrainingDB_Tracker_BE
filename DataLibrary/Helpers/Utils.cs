using System.Globalization;

namespace DataLibrary.Helpers;
internal static class Utils
{
    /// <summary>
    /// Normalizes the input string by: trimming it of white spaces then lowering its casing.
    /// </summary>
    /// <param name="string">the string you want normalized.</param>
    /// <returns>a normalized string. example: " Can aaN " -> "can aan".</returns>
    internal static string NormalizeString(string @string)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToLower(@string.Trim());
    }

    /// <summary>
    /// Tries to parse a date string via a list of predefined formats. 
    /// </summary>
    /// <param name="dateString">a nullable string. </param>
    /// <returns>a nullable DateTime object from the input string.</returns>
    internal static DateTime? ParseDate(string? dateString)
    {
        string[] formats = {
        "d-M-yyyy HH:mm:ss",
        "M-d-yyyy HH:mm:ss",
        "M-d-yyyy",
        "d-M-yyyy"
        };

        if (
            DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime parsedDate)
            )
            return parsedDate;

        return null;
    }
}
