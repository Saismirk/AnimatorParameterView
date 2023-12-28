using System.Reflection;
using System.Text.RegularExpressions;

namespace AnimatorParameterViewGenerator;

public static class StringExtensions {
    public static string[] Split(this string str, string separator) {
        return str.Split(new[] { separator }, StringSplitOptions.None);
    }

    public static long ToLong(this string str) => long.TryParse(str, out var result) ? result : 0;
    public static int ToInt(this string str) => int.TryParse(str, out var result) ? result : 0;
    public static bool ToBool(this string str) => bool.TryParse(str, out var result) && result;
}