using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Breakforge.Effects;

namespace Breakforge.Core;

public interface ISystem
{
    void Update(World world, float dt) { }
    void Draw(World world, SpriteBatch sb) { }
}

/// <summary>
/// Holds all entities and systems for the current play session. The World is
/// re-created when you start a new level — keep no long-lived state here.
/// </summary>
public sealed class World
{
    public EventBus Bus { get; } = new();
    public List<Entity> Entities { get; } = new();
    public List<ISystem> Systems { get; } = new();

    public Microsoft.Xna.Framework.Rectangle PlayField { get; set; }
    public EffectStack GlobalEffects { get; } = new();

    private readonly List<Entity> _spawnQueue = new();

    public Entity Spawn(Entity e)
    {
        _spawnQueue.Add(e);
        return e;
    }

    public IEnumerable<Entity> WithKind(EntityKind kind)
    {
        foreach (var e in Entities) if (e.IsAlive && e.Kind == kind) yield return e;
    }

    public void Update(float dt)
    {
        // Flush spawn queue first so newly spawned entities tick this frame.
        if (_spawnQueue.Count > 0)
        {
            Entities.AddRange(_spawnQueue);
            _spawnQueue.Clear();
        }

        GlobalEffects.Tick(dt, this);

        foreach (var sys in Systems) sys.Update(this, dt);

        // Behavior tick (entities own their behaviors; systems handle cross-cutting).
        foreach (var e in Entities)
        {
            if (!e.IsAlive) continue;
            e.FlushBehaviorChanges(this);
            foreach (var b in e.Behaviors) b.OnUpdate(e, this, dt);
        }
    }

    public void Draw(SpriteBatch sb)
    {
        foreach (var sys in Systems) sys.Draw(this, sb);
        foreach (var e in Entities)
        {
            if (!e.IsAlive) continue;
            foreach (var b in e.Behaviors) b.OnDraw(e, this, sb);
        }
    }
}
