using System;
using System.Text;

namespace AnimatorParameterViewGenerator; 

class AnimatorControllerParameterTriggerProvider : AnimatorControllerParameterProvider {
    public override void Generate(StringBuilder sourceBuilder, string paramName) {
        var target = "Target";
        var identifierName = Utils.ToPropertyName(paramName);

        sourceBuilder.AppendLine($@"
        /// <summary>
        /// [Source-Generated] Triggers Animator Trigger Parameter ({paramName}).
        /// </summary>
        public void Trigger{identifierName}() {{
            {target}.SetTrigger({identifierName}Hash);
        }}

        /// <summary>
        /// [Source-Generated] Resets Animator Trigger Parameter ({paramName}).
        /// </summary>
        public void Reset{identifierName}() {{
            {target}.ResetTrigger({identifierName}Hash);
        }}");
    }
}