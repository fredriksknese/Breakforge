using Breakforge.Core;
using Breakforge.Behaviors;

namespace Breakforge.Effects;

/// <summary>
/// While active, all balls deal area-damage when they hit a brick.
/// Wires up a BallExplosiveBehavior to every ball that exists when applied,
/// and removes it on expiration.
/// </summary>
public sealed class ExplosiveBallEffect : Effect
{
    public override string Id => "ball.explosive";
    public float Radius { get; init; } = 64f;
    public int Damage { get; init; } = 1;

    private readonly System.Collections.Generic.List<(Entity, BallExplosiveBehavior)> _attached = new();

    public override void OnApply(World world)
    {
        foreach (var ball in world.WithKind(EntityKind.Ball))
        {
            var b = new BallExplosiveBehavior { Radius = Radius, Damage = Damage };
            ball.AddBehavior(b);
            _attached.Add((ball, b));
        }
    }

    public override void OnRemove(World world)
    {
        foreach (var (e, b) in _attached)
            if (e.IsAlive) e.RemoveBehavior(b);
        _attached.Clear();
    }
}
