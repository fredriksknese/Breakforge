using System.Collections.Generic;
using Breakforge.Core;

namespace Breakforge.Effects;

/// <summary>
/// Time-bounded modifier applied to the world, a paddle, or a ball.
/// Powerups translate to one or more effects. Effects are the canonical
/// way to express "while active, change something" — width, speed, ball
/// size, ball-explodes-on-hit, etc.
/// </summary>
public interface IEffect
{
    string Id { get; }                    // e.g. "paddle.width", "ball.size"
    float RemainingSeconds { get; set; }  // float.PositiveInfinity for permanent
    bool IsAlive { get; }
    Entity? Target { get; set; }          // null = global

    void OnApply(World world);
    void OnRemove(World world);
    void Tick(float dt, World world);
}

public abstract class Effect : IEffect
{
    public abstract string Id { get; }
    public float RemainingSeconds { get; set; } = 8f;
    public bool IsAlive => RemainingSeconds > 0f;
    public Entity? Target { get; set; }

    public virtual void OnApply(World world) { }
    public virtual void OnRemove(World world) { }
    public virtual void Tick(float dt, World world) => RemainingSeconds -= dt;
}

/// <summary>
/// Holds effects and ticks them. Stacking rule per effect-id is set on the
/// effect itself; the default is "refresh duration when reapplied".
/// </summary>
public sealed class EffectStack
{
    private readonly List<IEffect> _effects = new();
    public IReadOnlyList<IEffect> Active => _effects;

    public void Add(IEffect e, World world)
    {
        // Refresh-by-id: if the same id is on the same target, take max remaining.
        foreach (var existing in _effects)
        {
            if (existing.Id == e.Id && ReferenceEquals(existing.Target, e.Target))
            {
                if (e.RemainingSeconds > existing.RemainingSeconds)
                    existing.RemainingSeconds = e.RemainingSeconds;
                return;
            }
        }
        _effects.Add(e);
        e.OnApply(world);
    }

    public bool Has(string id)
    {
        foreach (var e in _effects) if (e.Id == id) return true;
        return false;
    }

    public void Tick(float dt, World world)
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var e = _effects[i];
            e.Tick(dt, world);
            if (!e.IsAlive)
            {
                e.OnRemove(world);
                _effects.RemoveAt(i);
            }
        }
    }
}
