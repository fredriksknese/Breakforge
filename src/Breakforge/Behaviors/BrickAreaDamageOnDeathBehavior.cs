using System;
using Microsoft.Xna.Framework;
using Breakforge.Core;
using Breakforge.Gameplay;

namespace Breakforge.Behaviors;

/// <summary>
/// Bomb brick: on death, emits an AreaDamageEvent that damages all bricks in
/// Radius. Chain reactions: another bomb brick caught in the radius will then
/// emit its own AreaDamageEvent on death, etc.
/// </summary>
public sealed class BrickAreaDamageOnDeathBehavior : IBehavior
{
    public float Radius { get; init; } = 96f;
    public int Damage { get; init; } = 99; // tuneable

    private IDisposable? _sub;
    private Entity? _self;
    private World? _world;

    public void OnAttach(Entity self, World world)
    {
        _self = self;
        _world = world;
        _sub = world.Bus.Subscribe<BrickDestroyedEvent>(OnDestroyed);
    }

    public void OnDetach(Entity self, World world)
    {
        _sub?.Dispose();
        _sub = null;
    }

    private void OnDestroyed(BrickDestroyedEvent e)
    {
        if (_self is null || _world is null) return;
        if (!ReferenceEquals(e.Brick, _self)) return;
        var pos = _self.Get<Transform>().Position;
        Spawning.SpawnFx(_world, pos, new Color(255, 140, 60), Radius, 0.35f);
        _world.Bus.Publish(new AreaDamageEvent(pos, Radius, Damage, _self));
    }
}
