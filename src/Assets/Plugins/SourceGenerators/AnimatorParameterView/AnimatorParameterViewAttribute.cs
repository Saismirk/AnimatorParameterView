using System;

[AttributeUsage(AttributeTargets.Class)]
public class AnimatorParameterViewAttribute : Attribute {
    /// <summary>
    /// Marks a class as a container for Animator Parameters. The class must be a partial class. The property Target must be initialized to the Animator instance.
    /// </summary>
    /// <param name="controllerPath">The path to the Animator Controller. This path is relative to the location of the class file.</param>
    public AnimatorParameterViewAttribute(string controllerPath) { }
}