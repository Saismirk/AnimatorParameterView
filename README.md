# AnimatorParameterView

Automatically generates utility methods for setting and getting Unity Animator parameters, as well as triggering and resetting triggers.
It can also generate methods for cross-fading between animation states.
This plugin makes use of C# Source Generators.

## For Parameters

### Usage

1. Add the `AnimatorParameterView` attribute to a class.
2. Set the path to the target Animator Controller file as the parameter of the attribute.
   The path is relative to the path of the class. e.g. `"../Resources/Animators/PlayerController.controller"`.
3. Set the class as `partial`.
4. Wait for Unity to recompile the project.
5. Within the class, initialize the generated property `Target` to a reference of the target Animator component.

### Generated Properties and Methods by Parameter Type

The plugins generates a partial class with the same name as the class it is attached to.
The class contains a property named `Target` of type `Animator` and a set of methods for each parameter in the Animator Controller.

| Parameter Type | Public Properties       | Public Methods                                                          |
|----------------|-------------------------|-------------------------------------------------------------------------|
| Bool           | `bool <ParameterName>`  |                                                                         |
| Int            | `int <ParameterName>`   |                                                                         |
| Float          | `float <ParameterName>` | `void Set<ParameterName>(float value, float dampTime, float deltaTime)` |
| Trigger        |                         | `void Trigger<ParameterName>()`<br/> `void Reset<ParameterName>()`      |

### Example

```csharp
[AnimatorParameterView("../Resources/Animators/PlayerController.controller")]
public partial class PlayerController : MonoBehaviour {
    private void Awake() {
        Target = GetComponent<Animator>();
    }

    private void OnJump(InputAction.CallbackContext context) {
        if (context.performed) {
            TriggerJump();
        }
    }
}
```

In this example, the `void TriggerJump()` method was generated by the source generator from a parameter named `jump` in the Animator Controller.

## For States

### Usage

Similar to the usage for parameters, but add the `AnimatorStateView` attribute instead.

### Generated Properties and Methods

The plugin generates an enum of type `<ClassName>AnimatorState` for the states in the Animator Controller.
The enum contains the full names of the states in the Animator Controller in PascalCase.

The plugin also generates a partial class with the same name as the class it is attached to.
If the attribute `AnimatorParameterView` is also attached to the class,
the property `Target` will remain in the `AnimatorParameterView` generated partial class.

The method `void CrossFade(<ClassName>AnimatorState state, float transitionDuration, int layer, float normalizedTimeOffset)` is generated, where the enum is 
used to specify the state to cross-fade to.

### Example

```csharp
[AnimatorStateView("../Resources/Animators/PlayerController.controller")]
public partial class PlayerController : MonoBehaviour {
    private void Awake() {
        Target = GetComponent<Animator>();
    }

    private void OnJump(InputAction.CallbackContext context) {
        if (context.performed) {
            CrossFade(PlayerControllerAnimatorState.BaseLayerJump, 0.1f);
        }
    }
}
```