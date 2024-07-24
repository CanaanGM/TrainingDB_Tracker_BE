using System.Globalization;

namespace DataLibrary.Helpers;
public static class Utils
{
    /// <summary>
    /// Normalizes the input string by: trimming it of white spaces then lowering its casing.
    /// </summary>
    /// <param name="string">the string you want normalized.</param>
    /// <returns>a normalized string. example: " Can aaN " -> "can aan".</returns>
    /// <exception cref="ArgumentException">on incorrect input, an argument Exception is thrown!</exception>
    public static string NormalizeString(string @string)
    {
        if (string.IsNullOrEmpty(@string)) throw new ArgumentException($"Incorrect input passed: {@string}.");
        return CultureInfo.CurrentCulture.TextInfo.ToLower(@string.Trim());
    }

    /// <summary>
    /// Tries to parse a date string via a list of predefined formats. 
    /// </summary>
    /// <param name="dateString">a nullable string. of the format month-day-year or month-day-year hour:minute:second </param>
    /// <returns>a nullable DateTime object from the input string.</returns>
    public static DateTime? ParseDate(string? dateString)
    {
        string[] formats = {
        "M-d-yyyy HH:mm:ss",
        "M-d-yyyy",
        };

        if (
            DateTime.TryParseExact(dateString, formats, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out DateTime parsedDate)
            )
            return parsedDate;

        return null;
    }

    /// <summary>
    /// turns minutes into seconds
    /// </summary>
    /// <param name="durationInMinutes">nullable duration in minutes</param>
    /// <returns>duration in seconds or null.</returns>
    public static int? DurationSecondsFromMinutes(int? durationInMinutes) => durationInMinutes is not null ? durationInMinutes * 60 : null;

    /// <summary>
    /// Takes in a list of exercises and return the same list but with a <seealso cref="NormalizeString(string)"/> normalized names
    /// </summary>
    /// <param name="listOfStrings">an ICollection of strings to be normalized</param>
    /// <returns>a distinct list of normalized names</returns>
    public static List<string> NormalizeStringList(ICollection<string> listOfStrings)
    {
        return listOfStrings
            .Select(x => Utils.NormalizeString(x))
            .Distinct()
            .ToList();
    }
}
