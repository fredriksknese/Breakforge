using System;
using Microsoft.Xna.Framework;
using Breakforge.Core;
using Breakforge.Gameplay;

namespace Breakforge.Behaviors;

/// <summary>
/// When the ball hits a brick, publish an AreaDamageEvent at impact. The
/// CollisionSystem dispatches BallHitBrickEvent that we listen for.
/// Attached by ExplosiveBallEffect; removed when the effect expires.
/// </summary>
public sealed class BallExplosiveBehavior : IBehavior
{
    public float Radius { get; init; } = 64f;
    public int Damage { get; init; } = 1;

    private IDisposable? _sub;
    private Entity? _self;
    private World? _world;

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
        _self = null;
        _world = null;
    }

    private void OnHit(BallHitBrickEvent e)
    {
        if (_self is null || _world is null) return;
        if (!ReferenceEquals(e.Ball, _self)) return;
        var pos = e.Brick.Get<Transform>().Position;
        Spawning.SpawnFx(_world, pos, Color.OrangeRed, Radius, 0.25f);
        _world.Bus.Publish(new AreaDamageEvent(pos, Radius, Damage, _self));
    }
}
