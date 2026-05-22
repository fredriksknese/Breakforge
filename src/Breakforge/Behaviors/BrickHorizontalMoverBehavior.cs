using Microsoft.Xna.Framework;
using Breakforge.Core;

namespace Breakforge.Behaviors;

/// <summary>
/// Brick slides side-to-side within the playfield. Demonstrates a moving brick
/// without dedicating a special component or entity type.
/// </summary>
public sealed class BrickHorizontalMoverBehavior : IBehavior
{
    public float Speed { get; init; } = 60f;
    public float HalfRange { get; init; } = 80f;

    private Vector2 _origin;
    private float _phase;
    private bool _initialized;

    public void OnUpdate(Entity self, World world, float dt)
    {
        var t = self.Get<Transform>();
        if (!_initialized) { _origin = t.Position; _initialized = true; }
        _phase += dt * (Speed / System.Math.Max(1f, HalfRange));
        float x = _origin.X + System.MathF.Sin(_phase) * HalfRange;
        t.Position.X = MathHelper.Clamp(x,
            world.PlayField.Left + t.Size.X * 0.5f,
            world.PlayField.Right - t.Size.X * 0.5f);
    }
}
