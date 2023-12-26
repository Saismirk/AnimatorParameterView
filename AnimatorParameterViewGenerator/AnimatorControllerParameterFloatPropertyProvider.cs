using System.Text;

namespace AnimatorParameterViewGenerator; 

class AnimatorControllerParameterFloatPropertyProvider : AnimatorControllerParameterPropertyProvider {
    public AnimatorControllerParameterFloatPropertyProvider() : base("float", "{0}.GetFloat({1})",
        "{0}.SetFloat({1}, {2})") {
    }

    public override void Generate(StringBuilder sourceBuilder, string paramName) {
        const string target = "Target";
        var identifierName = Utils.ToPropertyName(paramName);

        sourceBuilder.AppendLine($@"
        /// <summary>
        /// [Source-Generated] Sets Animator Float parameter ({paramName}) over time with dampening.
        /// </summary>
        public void Set{identifierName}(float value, float dampTime, float deltaTime) {{
            {target}.SetFloat({identifierName}Hash, value, dampTime, deltaTime);
        }}");
    }
}