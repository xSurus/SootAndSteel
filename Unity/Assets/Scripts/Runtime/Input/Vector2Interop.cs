namespace Gamelab.Input.Runtime
{
    /// <summary>
    /// System.Numerics.Vector2 and UnityEngine.Vector2 share the name "Vector2", so any
    /// Gamelab.Runtime code needing both must fully qualify one. Vector2Raw exists only
    /// to avoid a same-named-type ambiguity error at call sites that convert a
    /// UnityEngine.Vector2 read from the Input System into the System.Numerics.Vector2
    /// that Gamelab.Input.IInputActions requires.
    /// </summary>
    public readonly struct Vector2Raw
    {
        public readonly System.Numerics.Vector2 Value;
        public Vector2Raw(System.Numerics.Vector2 value) => Value = value;
    }

    public static class Vector2InteropExtensions
    {
        public static Vector2Raw ToSystemVector2(this UnityEngine.Vector2 v) =>
            new Vector2Raw(new System.Numerics.Vector2(v.x, v.y));
    }
}
