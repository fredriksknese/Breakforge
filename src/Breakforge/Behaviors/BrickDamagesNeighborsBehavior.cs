using System;
using Microsoft.Xna.Framework;
using Breakforge.Core;

namespace Breakforge.Behaviors;

/// <summary>
/// When this brick is damaged (not necessarily killed), it also damages the
/// nearest N bricks within Radius. Demonstrates brick-damages-brick chain
/// reactions distinct from the bomb-on-death pattern.
/// </summary>
public sealed class BrickDamagesNeighborsBehavior : IBehavior
{
    public float Radius { get; init; } = 90f;
    public int Damage { get; init; } = 1;
    public int MaxTargets { get; init; } = 2;

    private IDisposable? _sub;
    private Entity? _self;
    private World? _world;
    private bool _firing; // re-entrancy guard

    public void OnAttach(Entity self, World world)
    {
        _self = self;
        _world = world;
        _sub = world.Bus.Subscribe<BrickDamagedEvent>(OnDamaged);
    }

    public void OnDetach(Entity self, World world)
    {
        _sub?.Dispose();
        _sub = null;
    }

    private void OnDamaged(BrickDamagedEvent e)
    {
        if (_firing || _self is null || _world is null) return;
        if (!ReferenceEquals(e.Brick, _self)) return;
        _firing = true;
        try
        {
            var origin = _self.Get<Transform>().Position;
            int hits = 0;
            foreach (var other in _world.WithKind(EntityKind.Brick))
            {
                if (ReferenceEquals(other, _self) || !other.IsAlive) continue;
                var pos = other.Get<Transform>().Position;
                if (Vector2.DistanceSquared(pos, origin) > Radius * Radius) continue;
                var h = other.TryGet<Health>();
                if (h is null || h.Invulnerable) continue;
                h.Current -= Damage;
                _world.Bus.Publish(new BrickDamagedEvent(other, Damage, _self));
                if (++hits >= MaxTargets) break;
            }
        }
        finally { _firing = false; }
    }
}
