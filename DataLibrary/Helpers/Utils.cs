using System.Globalization;

namespace DataLibrary.Helpers;
public static class Utils
{
    /// <summary>
    /// Normalizes the input string by trimming whitespace and converting it to lowercase based on detected language.
    /// This method first determines the probable language of the input based on character ranges and then applies
    /// the appropriate cultural settings for case conversion.
    /// </summary>
    /// <param name="input">The string to be normalized.</param>
    /// <returns>A normalized string with whitespace trimmed and casing converted.</returns>
    /// <exception cref="ArgumentException">Thrown when the input string is null or empty.</exception>
    public static string NormalizeString(string input)
    {
        if (string.IsNullOrEmpty(input?.Trim()))
            return null;

        var detectedCulture = DetectCultureFromInput(input);
        var cultureInfo = new CultureInfo(detectedCulture);
        return input.Trim().ToLower(cultureInfo);
    }
    
    /// <summary>
    /// Detects the primary language of the input string based on the presence of specific Unicode characters.
    /// This method checks for characters within defined ranges for Arabic and Japanese scripts to infer the language.
    /// If no specific characters are found, it defaults to English.
    /// </summary>
    /// <param name="input">The string from which to detect the language.</param>
    /// <returns>The culture code (e.g., "en-US", "ar-SA", "ja-JP") corresponding to the detected language.</returns>
    private static string DetectCultureFromInput(string input)
    {
        // Check if the input contains extended Arabic or Japanese characters
        if (input.Any(c => c >= 0x0600 && c <= 0x06FF || c >= 0x0750 && c <= 0x077F))  // Arabic Unicode Range
            return "ar-SA"; // Saudi Arabia Arabic culture

        if (input.Any(c => (c >= 0x3040 && c <= 0x309F) || // Hiragana
                           (c >= 0x30A0 && c <= 0x30FF) || // Katakana
                           (c >= 0x4E00 && c <= 0x9FAF)))  // Kanji
            return "ja-JP"; // Japanese culture
        // Default to English if no specific characters are found
        return "en-US";
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
