using System.Text;

namespace AnimatorParameterViewGenerator; 

class AnimatorControllerParameterPropertyProvider : AnimatorControllerParameterProvider {
    public string CSharpTypeSyntax { get; }
    public string GetterFormat { get; }
    public string SetterFormat { get; }

    public AnimatorControllerParameterPropertyProvider(string cSharpTypeSyntax, string getterFormat,
        string setterFormat) {
        CSharpTypeSyntax = cSharpTypeSyntax;
        GetterFormat = getterFormat;
        SetterFormat = setterFormat;
    }

    public override void Generate(StringBuilder sourceBuilder, string paramName) {
        var target = "Target";
        var identifierName = Utils.ToPropertyName(paramName);
        var paramNameLiteral = $"{identifierName}Hash";

        sourceBuilder.AppendLine($@"
        /// <summary>
        /// [Source-Generated] Animator {Utils.ToPropertyName(CSharpTypeSyntax)} parameter ({paramName}).
        /// </summary>
        public {CSharpTypeSyntax} {identifierName} {{
            get => {string.Format(GetterFormat, target, paramNameLiteral)};
            set => {string.Format(SetterFormat, target, paramNameLiteral, "value")};
        }}");
    }
}