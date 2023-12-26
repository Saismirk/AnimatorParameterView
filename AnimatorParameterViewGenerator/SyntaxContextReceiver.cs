using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnimatorParameterViewGenerator; 

public class SyntaxContextReceiver : ISyntaxContextReceiver {
    internal static ISyntaxContextReceiver Create() => new SyntaxContextReceiver();

    public List<(ISymbol classSymbol, ClassDeclarationSyntax classSyntax)> AnimatorViews { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
        var node = context.Node;

        if (node is not ClassDeclarationSyntax classDeclaration || classDeclaration.AttributeLists.Count == 0) {
            return;
        }
            
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol?
                .GetAttributes()
                .Any(ad => ad.AttributeClass != null && ad.AttributeClass.ToDisplayString() == "AnimatorParameterViewAttribute") == true) {
            AnimatorViews.Add((classSymbol, classDeclaration));
        }
    }
}