using System.Globalization;

namespace DataLibrary.Helpers;
internal static class Utils
{
    internal static string NormalizeString(string @string)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToLower(@string.Trim());
    }
}
