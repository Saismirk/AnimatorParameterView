using System.Collections.Generic;
using System.Text;

namespace AnimatorParameterViewGenerator; 

internal abstract class AnimatorControllerParameterProvider {
    public static IReadOnlyDictionary<AnimatorControllerParameterType, AnimatorControllerParameterProvider>
        Providers { get; } = new Dictionary<AnimatorControllerParameterType, AnimatorControllerParameterProvider>() {
        {
            AnimatorControllerParameterType.Float,
            new AnimatorControllerParameterFloatPropertyProvider()
        }, {
            AnimatorControllerParameterType.Int,
            new AnimatorControllerParameterPropertyProvider("int", "{0}?.GetInteger({1}) ?? 0", "{0}?.SetInteger({1}, {2})")
        }, {
            AnimatorControllerParameterType.Bool,
            new AnimatorControllerParameterPropertyProvider("bool", "{0}?.GetBool({1}) ?? false", "{0}?.SetBool({1}, {2})")
        }, {
            AnimatorControllerParameterType.Trigger,
            new AnimatorControllerParameterTriggerProvider()
        }
    };
    
    public static void GenerateParameterHash(StringBuilder sourceBuilder, string paramName) {
        var identifierName = Utils.ToPropertyName(paramName);
        var paramNameLiteral = $"@\"{Utils.ToCSharpEscapedVerbatimLiteral(paramName)}\"";
        sourceBuilder.AppendLine($@"
        static int {paramName}Hash;
        static int {identifierName}Hash => {paramName}Hash != 0 
            ? {paramName}Hash 
            : {paramName}Hash = global::UnityEngine.Animator.StringToHash({paramNameLiteral});");
    }

    public abstract void Generate(StringBuilder sourceBuilder, string paramName);
}

public enum AnimatorControllerParameterType {
    Float = 1,
    Int = 3,
    Bool = 4,
    Trigger = 9,
}