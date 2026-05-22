using Microsoft.Xna.Framework;
using Breakforge.Core;
using Breakforge.Gameplay;

namespace Breakforge.Behaviors;

/// <summary>
/// One-shot: the first brick collision splits this ball into ExtraBalls clones
/// at small angle offsets. Demonstrates a per-ball persistent modifier (vs
/// the global ExplosiveBallEffect).
/// </summary>
public sealed class BallSplitOnFirstHitBehavior : IBehavior
{
    public int ExtraBalls { get; init; } = 2;

    private System.IDisposable? _sub;
    private Entity? _self;
    private World? _world;
    private bool _consumed;

    public void OnAttach(Entity self, World world)
    {
        _self = self;
        _world = world;
        _sub = world.Bus.Subscribe<BallHitBrickEvent>(OnHit);
    }

    public void OnDetach(Entity self, World world)
    {
        _sub?.Dispose();
        _sub = null;
    }

    private void OnHit(BallHitBrickEvent e)
    {
        if (_consumed || _self is null || _world is null) return;
        if (!ReferenceEquals(e.Ball, _self)) return;
        _consumed = true;

        var t = _self.Get<Transform>();
        var v = _self.Get<Velocity>();
        float r = _self.Get<CircleCollider>().Radius;

        for (int i = 0; i < ExtraBalls; i++)
        {
            float angDeg = 22f * (i + 1) * (i % 2 == 0 ? 1 : -1);
            var rot = Vector2.Transform(v.Value, Matrix.CreateRotationZ(MathHelper.ToRadians(angDeg)));
            Spawning.SpawnBall(_world, t.Position, rot, r);
        }
        _self.RemoveBehavior(this);
    }
}
