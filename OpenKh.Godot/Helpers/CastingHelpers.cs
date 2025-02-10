using Godot;

namespace OpenKh.Godot.Helpers;

public static class CastingHelpers
{
    public static Vector4 ToGodot(this System.Numerics.Vector4 vec) => new(vec.X, vec.Y, vec.Z, vec.W);
    public static System.Numerics.Vector4 ToSystem(this Vector4 vec) => new(vec.X, vec.Y, vec.Z, vec.W);
    
    public static Vector3 ToGodot(this System.Numerics.Vector3 vec) => new(vec.X, vec.Y, vec.Z);
    public static System.Numerics.Vector3 ToSystem(this Vector3 vec) => new(vec.X, vec.Y, vec.Z);
    
    public static Vector2 ToGodot(this System.Numerics.Vector2 vec) => new(vec.X, vec.Y);
    public static System.Numerics.Vector2 ToSystem(this Vector2 vec) => new(vec.X, vec.Y);

    //TODO: more swizzles
    public static Vector3 XYZ(this Vector4 vec) => new(vec.X, vec.Y, vec.Z);
    public static Vector2 XY(this Vector3 vec) => new(vec.X, vec.Y);
    public static Vector2 XZ(this Vector3 vec) => new(vec.X, vec.Z);
    public static System.Numerics.Vector3 XYZ(this System.Numerics.Vector4 vec) => new(vec.X, vec.Y, vec.Z);
    public static System.Numerics.Vector2 XY(this System.Numerics.Vector3 vec) => new(vec.X, vec.Y);
    public static System.Numerics.Vector2 XZ(this System.Numerics.Vector3 vec) => new(vec.X, vec.Z);
}
