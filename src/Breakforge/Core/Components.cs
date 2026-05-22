using Microsoft.Xna.Framework;

namespace Breakforge.Core;

/// <summary>Marker for components. Components are passive data — no behavior.</summary>
public interface IComponent { }

public sealed class Transform : IComponent
{
    public Vector2 Position;
    public Vector2 Size = new(16, 16);
    public float Rotation;
    public Rectangle Bounds => new(
        (int)(Position.X - Size.X * 0.5f),
        (int)(Position.Y - Size.Y * 0.5f),
        (int)Size.X,
        (int)Size.Y);
}

public sealed class Velocity : IComponent
{
    public Vector2 Value;
    public float MaxSpeed = 1200f;
}

public sealed class CircleCollider : IComponent
{
    public float Radius = 8f;
}

public sealed class BoxCollider : IComponent
{
    // Uses Transform.Size unless overridden.
    public Vector2? Override;
}

public sealed class SpriteRenderer : IComponent
{
    public Color Color = Color.White;
    public Color? OutlineColor;
    public int Layer; // higher = drawn later
    public bool Circle;
}

public sealed class Health : IComponent
{
    public int Max = 1;
    public int Current = 1;
    public bool Invulnerable;
    public bool IsDead => Current <= 0;
}

public sealed class Lifetime : IComponent
{
    public float Remaining;
}

/// <summary>Identifies which definition spawned this entity (brick id, powerup id, etc.).</summary>
public sealed class DefRef : IComponent
{
    public string DefId = "";
}
