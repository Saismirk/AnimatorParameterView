namespace AnimatorParameterViewGenerator;

public static class StringExtensions {
    public static string[] Split(this string str, string separator) {
        return str.Split(new[] {separator}, StringSplitOptions.None);
    }
}