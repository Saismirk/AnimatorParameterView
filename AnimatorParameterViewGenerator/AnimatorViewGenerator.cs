using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace AnimatorParameterViewGenerator;

[Generator]
public class AnimatorViewGenerator : ISourceGenerator {
    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(SyntaxContextReceiver.Create);
    }

    public void Execute(GeneratorExecutionContext context) {
        if (context.SyntaxContextReceiver is not SyntaxContextReceiver receiver) return;
        var targetWasAdded = ProcessAttribute("AnimatorParameterView", context, receiver);
        targetWasAdded = ProcessAttribute("AnimatorStateView", context, receiver, !targetWasAdded);
        ProcessAttribute("AnimatorStateMachineView", context, receiver, !targetWasAdded);
    }

    private static bool ProcessAttribute(string attribute, GeneratorExecutionContext context, SyntaxContextReceiver receiver, bool addTarget = true) {
        var attributeSymbol = context.Compilation.GetTypeByMetadataName($"{attribute}Attribute") ?? null;
        if (attributeSymbol == null) return false;
        foreach (var classData in receiver.AnimatorViews) {
            var classSource = ProcessClass(classData.classSymbol, attributeSymbol, classData.classSyntax, addTarget);
            if (string.IsNullOrEmpty(classSource)) continue;
            var namespaceName = classData.classSymbol.ContainingNamespace?.Name ?? "global";
            var className = classData.classSymbol.Name;
            context.AddSource($"{namespaceName}_{className}_{attribute}.g.cs", classSource);
#if DEBUG
            Utils.SaveSourceToPath($"E:/{namespaceName}_{className}_{attribute}.g.txt", classSource);
#endif
        }

        return true;
    }

    private static string ProcessClass(ISymbol classSymbol, ISymbol? attributeSymbol, ClassDeclarationSyntax classSyntax, bool addTarget = true) {
        var source = new StringBuilder();

        try {
            if (!GenerateClass(classSymbol, attributeSymbol, classSyntax, source, addTarget)) return string.Empty;
        }
        catch (Exception e) {
            source.AppendLine($@"/*Error: {e.Message}*/");
        }

        return source.ToString();
    }

    private static bool GenerateClass(
        ISymbol classSymbol,
        ISymbol? attributeSymbol,
        ClassDeclarationSyntax classSyntax,
        StringBuilder sourceBuilder,
        bool addTarget = true) {
        var tempParams = new List<AnimatorControllerParameter>();
        var states = new HashSet<AnimatorState>();
        var attributeData = classSymbol.GetAttributes()
                                       .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
        if (attributeData == null) return false;

        var animatorControllerPath = attributeData.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(animatorControllerPath)) return false;

        var animatorFullPath = AnimatorParser.ParseAnimatorController(classSyntax,
                                                                      animatorControllerPath ?? string.Empty,
                                                                      tempParams,
                                                                      ref states);
        var namespaceSymbol = classSymbol.ContainingNamespace;
        if (!namespaceSymbol.IsGlobalNamespace) {
            sourceBuilder.AppendLine($@"using System.Collections.Generic;

namespace {namespaceSymbol} {{");
        }

        sourceBuilder.AppendLine($@"    public partial class {classSymbol.Name} {{//From: {animatorFullPath}");
        if (addTarget) sourceBuilder.AppendLine($@"        global::UnityEngine.Animator Target {{ get; set; }}");
        switch (attributeSymbol?.Name) {
            case "AnimatorParameterViewAttribute":
                sourceBuilder.AppendLine($@"        //Detected Parameters: {tempParams.Count}");
                AppendParameters(sourceBuilder, tempParams);
                break;
            case "AnimatorStateViewAttribute":
                sourceBuilder.AppendLine($@"        //Detected States Machines: {states.Count}");
                AppendStateMachines(sourceBuilder, classSymbol, states);
                break;
        }

        sourceBuilder.AppendLine(namespaceSymbol.IsGlobalNamespace ? "}" : "\t}");
        if (!namespaceSymbol.IsGlobalNamespace) {
            sourceBuilder.AppendLine("}");
        }

        return true;
    }

    private static void AppendParameters(StringBuilder sourceBuilder, List<AnimatorControllerParameter> tempParams) {
        foreach (var param in tempParams) {
            if (!AnimatorControllerParameterProvider.Providers.TryGetValue(param.Type, out var provider)) continue;
            try {
                AnimatorControllerParameterProvider.GenerateParameterHash(sourceBuilder, param.Name);
                provider.Generate(sourceBuilder, param.Name);
            }
            catch (Exception e) {
                sourceBuilder.AppendLine($@"/*Error: {e.Message}*/");
            }
        }
    }

    private static void AppendStateMachines(StringBuilder sourceBuilder, ISymbol classSymbol, HashSet<AnimatorState> states) {
        sourceBuilder.AppendLine($@"        public enum {classSymbol.Name}AnimatorState {{");
        foreach (var state in states) {
            sourceBuilder.AppendLine($@"            {state.GetFullPropertyName()},");
        }

        sourceBuilder.AppendLine($@"        }}
");

        sourceBuilder.AppendLine($@"        private static readonly IReadOnlyDictionary<{classSymbol.Name}AnimatorState, int> STATE_HASH_DICTIONARY = 
            new Dictionary<{classSymbol.Name}AnimatorState, int> {{");
        foreach (var state in states) {
            sourceBuilder.AppendLine(
                $@"            {{ {classSymbol.Name}AnimatorState.{state.GetFullPropertyName()}, global::UnityEngine.Animator.StringToHash(""{state.GetFullName()}"") }},");
        }

        sourceBuilder.AppendLine($@"        }};");

        sourceBuilder.Append($@"
        /// <summary>
        ///   <para>Creates a crossfade from the current state to any other state using normalized times.</para>
        /// </summary>
        /// <param name=""state"">The state enum to crossfade to.</param>
        /// <param name=""normalizedTransitionDuration"">The duration of the transition (normalized).</param>
        /// <param name=""layer"">The layer where the crossfade occurs.</param>
        /// <param name=""normalizedTimeOffset"">The time of the state (normalized).</param>
        /// <param name=""normalizedTransitionTime"">The time of the transition (normalized).</param>
        public void CrossFade({classSymbol.Name}AnimatorState state, float normalizedTransitionDuration, int layer = -1, float normalizedTimeOffset = 0.0f, float normalizedTransitionTime = 0.0f) =>
            Target?.CrossFade(STATE_HASH_DICTIONARY[state], normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);");
    }
}