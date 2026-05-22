using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Breakforge.Core;
using Breakforge.Gameplay;

namespace Breakforge.Effects;

/// <summary>
/// One-shot effect: spawns N extra balls from every existing ball.
/// Implemented as an effect (not a one-off call) so it composes with
/// the same powerup pipeline as the timed effects.
/// </summary>
public sealed class MultiBallEffect : Effect
{
    public override string Id => "ball.multi";
    public int ExtraBalls { get; init; } = 2;

    public override void OnApply(World world)
    {
        var existing = new List<Entity>(world.WithKind(EntityKind.Ball));
        foreach (var src in existing)
        {
            var t = src.Get<Transform>();
            var v = src.Get<Velocity>();
            for (int i = 0; i < ExtraBalls; i++)
            {
                var angle = MathHelper.ToRadians(20f * (i + 1) * (i % 2 == 0 ? 1 : -1));
                var rotated = Vector2.Transform(v.Value, Matrix.CreateRotationZ(angle));
                Spawning.SpawnBall(world, t.Position, rotated, src.Get<CircleCollider>().Radius);
            }
        }
        RemainingSeconds = 0.01f; // one-shot, expires next tick
    }
}
