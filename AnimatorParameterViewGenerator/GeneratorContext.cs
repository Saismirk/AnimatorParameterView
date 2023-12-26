using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AnimatorParameterViewGenerator; 

public class GeneratorContext {
    public SyntaxContextReceiver Collector { get; }
    public Compilation Compilation { get; }
    public INamedTypeSymbol? AnimatorViewAttribute { get; }


    public GeneratorContext(Compilation compilation, SyntaxContextReceiver collector) {
        Collector = collector;

        Compilation = compilation;
        AnimatorViewAttribute = GetSymbol("AnimatorParameterViewAttribute");
        return;

        INamedTypeSymbol? GetSymbol(string metadataName) => compilation.GetTypeByMetadataName(metadataName);
    }
}