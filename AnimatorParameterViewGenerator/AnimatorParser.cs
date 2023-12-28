using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace AnimatorParameterViewGenerator;

public static class AnimatorParser {
    public const string STATE_REGEX = @"--- !u!110(?<type>2|7)\s&(?<hash>[-]?\d+)\s*\n(?:.*\n\s.)+m_Name:\s(?<name>.*)(?:.*\n\s.)+";
    public const string STATE_HASH_REGEX = @"\s*m_State(?:Machine)?:\s{fileID:\s(?<hash>[-]?\d*)}";

    public static HashSet<AnimatorState> ParseAnimatorStates(string yaml) {
        var matchGroups = Regex.Matches(yaml, STATE_REGEX)
                               .Cast<Match>()
                               .GroupBy(match => match.Groups["type"].Value)
                               .ToArray();

        var stateNameMap = matchGroups.SelectMany(match => match)
                                         .ToDictionary(match => match.Groups["hash"].Value.ToLong(),
                                                       match => match.Groups["name"].Value);
        
        var stateMatches = matchGroups.SelectMany(match => match)
                                      .ToArray();

        var stateDictionary = stateMatches.ToDictionary(match => match.Groups["hash"].Value.ToLong(),
                                                        match => match.Groups[0].Value);

        var stateMachines = stateMatches.Select(match => match.Groups["hash"].Value.ToLong())
                                        .Select(hash => new AnimatorState(hash, stateNameMap[hash], null))
                                        .ToHashSet();

        foreach (var stateMachine in stateMachines) {
            ParentChildren(stateDictionary[stateMachine.Hash], stateMachine, stateMachines);
        }

        return stateMachines;
    }

    private static void ParentChildren(
        string stateMachine,
        AnimatorState stateMachineNode,
        IReadOnlyCollection<AnimatorState> stateMachines) {
        var children = Regex.Matches(stateMachine, STATE_HASH_REGEX)
                            .Cast<Match>()
                            .Select(match => match.Groups["hash"].Value.ToLong())
                            .Select(hash => stateMachines.FirstOrDefault(node => node.Hash == hash))
                            .Where(node => node is not null);
        foreach (var child in children) {
            child!.ParentTo(stateMachineNode);
            stateMachineNode.Children.Add(child);
        }
    }

    public static string ParseAnimatorController(SyntaxNode animatorViewType,
        string animatorControllerPath,
        ICollection<AnimatorControllerParameter> tempParams,
        ref HashSet<AnimatorState> stateMachines) {
        var basePath = Path.GetDirectoryName(animatorViewType.SyntaxTree.FilePath) ?? string.Empty;
        var combinedPath = Path.Combine(basePath, animatorControllerPath);
        var animatorControllerFullPath = Path.GetFullPath(combinedPath);
        if (!File.Exists(animatorControllerFullPath)) {
            return "File not found at: " + animatorControllerFullPath + "\n";
        }

        string content;
        try {
            using var contentReader = new StreamReader(animatorControllerFullPath, Encoding.UTF8);
            content = contentReader.ReadToEnd();
        }
        catch (Exception e) {
            return "Error reading file: " + animatorControllerFullPath + "\n" + e.Message;
        }

        var rawParameters = content.Split("m_AnimatorParameters:")[1]
                                   .Split("m_AnimatorLayers:")[0];
        ParseAnimatorParameters(rawParameters, tempParams);
        stateMachines = ParseAnimatorStates(content);

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

public class AnimatorState {
    public long Hash { get; }
    public string Name { get; }
    public AnimatorState? Parent { get; set; }
    public HashSet<AnimatorState> Children { get; }
    
    public bool HasChildren => Children.Count > 0;

    public AnimatorState(long hash, string name, AnimatorState? parent) {
        Hash = hash;
        Name = name;
        Parent = parent;
        Children = new HashSet<AnimatorState>();
    }

    public AnimatorState ParentTo(AnimatorState parent) {
        Parent = parent;
        parent.Children.Add(this);
        return this;
    }

    public string GetFullName() {
        var name = Name;
        var parent = Parent;
        while (parent is not null) {
            name = $"{parent.Name}.{name}";
            parent = parent.Parent;
        }

        return name;
    }

    public string GetFullPropertyName() {
        var name = Utils.ToPropertyName(Name);
        var parent = Parent;
        while (parent is not null) {
            name = $"{Utils.ToPropertyName(parent.Name)}{name}";
            parent = parent.Parent;
        }

        return name.Replace(" ", "");
    }

    public override bool Equals(object? obj) => obj is AnimatorState state && Hash == state.Hash;
    protected bool Equals(AnimatorState other) => Hash == other.Hash;
    public override int GetHashCode() => Hash.GetHashCode();
    public static bool operator ==(AnimatorState left, AnimatorState right) => left.Equals(right);
    public static bool operator ==(AnimatorState left, long right) => left.Hash == right;
    public static bool operator ==(long left, AnimatorState right) => left == right.Hash;
    public static bool operator !=(AnimatorState left, AnimatorState right) => !(left == right);
    public static bool operator !=(AnimatorState left, long right) => !(left == right);
    public static bool operator !=(long left, AnimatorState right) => !(left == right);
}