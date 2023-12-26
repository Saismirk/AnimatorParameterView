namespace AnimatorParameterViewGenerator; 

public class AnimatorControllerParameter {
    public AnimatorControllerParameterType Type { get; }
    public string Name { get; }

    public AnimatorControllerParameter(AnimatorControllerParameterType type, string name) {
        Type = type;
        Name = name;
    }
}