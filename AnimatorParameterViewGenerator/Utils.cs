using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;

namespace AnimatorParameterViewGenerator; 

internal static class Utils {
    public static void SaveSourceToPath(string path, string source) {
        using var writer = new StreamWriter(path, false, Encoding.UTF8);
        writer.Write(source);
    }
    
    public static string ToPropertyName(string value) {
        var span = value.AsSpan();
        if (span.IsEmpty) return string.Empty;
        var hasIdentifier = !char.IsLetterOrDigit(span[0]);
        var length = hasIdentifier ? span.Length - 1 : span.Length;
        Span<char> resultSpan = stackalloc char[length];
        span.Slice(hasIdentifier ? 1 : 0)
            .CopyTo(resultSpan);
        resultSpan[0] = char.ToUpper(resultSpan[0]);
        return resultSpan.ToString();
    }

    public static string ToCSharpEscapedVerbatimLiteral(string value) {
        return value.Replace("\"", "\"\"");
    }
}