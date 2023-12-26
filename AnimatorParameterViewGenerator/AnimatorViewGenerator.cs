using System.Text;
using System;
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
        var attributeSymbol = context.Compilation.GetTypeByMetadataName("AnimatorParameterViewAttribute") ?? null;
        if (attributeSymbol == null) return;
        foreach (var classData in receiver.AnimatorViews) {
            var classSource = ProcessClass(classData.classSymbol, attributeSymbol, classData.classSyntax);
            var namespaceName = classData.classSymbol.ContainingNamespace?.Name ?? "global";
            var className = classData.classSymbol.Name;
            context.AddSource($"{namespaceName}_{className}_AnimatorParameterView.g.cs", classSource);
            //Utils.SaveSourceToPath($"E:/{namespaceName}_{className}_AnimatorParameterView.g.txt", classSource);
        }
    }
    
    private static string ProcessClass(ISymbol classSymbol, ISymbol? attributeSymbol, ClassDeclarationSyntax classSyntax) {
        var source = new StringBuilder();

        try {
            GenerateClass(classSymbol, attributeSymbol, classSyntax, source);
        } catch (Exception e) {
            source.AppendLine($@"/*Error: {e.Message}*/");
        }

        return source.ToString();
    }
    
    private static void GenerateClass(ISymbol classSymbol, ISymbol? attributeSymbol, ClassDeclarationSyntax classSyntax, StringBuilder sourceBuilder) {
        var tempParams = new List<AnimatorControllerParameter>();
        var attributeData = classSymbol
            .GetAttributes()
            .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
        if (attributeData == null) return;

        var animatorControllerPath = attributeData.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(animatorControllerPath)) return;

        var animatorFullPath = AnimatorParser.ParseAnimatorController(classSyntax, animatorControllerPath ?? string.Empty, tempParams);
        var namespaceSymbol = classSymbol.ContainingNamespace;
        if (!namespaceSymbol.IsGlobalNamespace) {
            sourceBuilder.AppendLine($@"namespace {namespaceSymbol} {{");
        }

        sourceBuilder.AppendLine($@"    public partial class {classSymbol.Name} {{
        global::UnityEngine.Animator Target {{ get; set; }} // Detected Parameters: {tempParams.Count} in {animatorFullPath}");
        foreach (var param in tempParams) {
            if (!AnimatorControllerParameterProvider.Providers.TryGetValue(param.Type, out var provider)) continue;
            try {
                AnimatorControllerParameterProvider.GenerateParameterHash(sourceBuilder, param.Name);
                provider.Generate(sourceBuilder, param.Name);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                sourceBuilder.AppendLine($@"/*Error: {e.Message}*/");
            }
        }

        sourceBuilder.AppendLine(namespaceSymbol.IsGlobalNamespace ? "}" : "\t}");
        if (!namespaceSymbol.IsGlobalNamespace) {
            sourceBuilder.AppendLine("}");
        }
    }
    
}