using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace AnimatorParameterViewGenerator;

public static class AnimatorParser {
    public static string ParseAnimatorController(SyntaxNode animatorViewType,
        string animatorControllerPath,
        ICollection<AnimatorControllerParameter> tempParams) {
        var basePath = Path.GetDirectoryName(animatorViewType.SyntaxTree.FilePath) ?? string.Empty;
        var combinedPath = Path.Combine(basePath, animatorControllerPath);
        var animatorControllerFullPath = Path.GetFullPath(combinedPath);
        if (!File.Exists(animatorControllerFullPath)) {
            return "File not found at: " + animatorControllerFullPath + "\n";
        }

        string content;
        try {
            using var contentReader = new StreamReader(animatorControllerFullPath, Encoding.UTF8);
            content = contentReader
                .ReadToEnd()
                .Split("m_AnimatorParameters:")[1]
                .Split("m_AnimatorLayers:")[0];
        }
        catch (Exception e) {
            return "Error reading file: " + animatorControllerFullPath + "\n" + e.Message;
        }

        ParseAnimatorParameters(content, tempParams);
        return animatorControllerFullPath;
    }

    private static void ParseAnimatorParameters(string content, ICollection<AnimatorControllerParameter> tempParams) {
        var names = GetYamlValue(content, "m_Name");
        var types = GetYamlValue(content, "m_Type");
        if (names.Length == 0) {
            throw new Exception("No parameters found");
        }

        if (names.Length != types.Length) {
            throw new Exception("Names and Types count mismatch");
        }

        for (var i = 0; i < names.Length; i++) {
            var name = names[i];
            var type = i < types.Length ? types[i] : "-1";
            if (!int.TryParse(type, out var result)) {
                continue;
            }

            if (result < 0) continue;
            tempParams.Add(new AnimatorControllerParameter((AnimatorControllerParameterType)result, name));
        }
    }
    
    private static string[] GetYamlValue(string content, string key) {
        try {
            var regex = new Regex($@"{key}:\s(?<value>[A-Za-z0-9]+)\n");
            var matches = regex.Matches(content);
            var result = new string[matches.Count];
            for (var i = 0; i < matches.Count; i++) {
                var match = matches[i];
                result[i] = match.Groups[1].Value;
            }

            return result;
        }
        catch (Exception e) {
            throw new Exception("Error parsing yaml: " + e.Message + "\n" + content + "\n" + key);
        }
    }
}