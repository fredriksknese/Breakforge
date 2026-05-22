using System;
using System.Collections.Generic;

namespace Breakforge.Core;

public enum EntityKind { Unknown, Paddle, Ball, Brick, Powerup, Wall, Fx }

/// <summary>
/// Bag of components and behaviors. The unit of gameplay.
/// Everything from a brick to a falling powerup to an explosion VFX is an Entity.
/// </summary>
public sealed class Entity
{
    private static int _nextId;
    public int Id { get; } = ++_nextId;
    public EntityKind Kind { get; set; } = EntityKind.Unknown;
    public bool IsAlive { get; private set; } = true;
    public HashSet<string> Tags { get; } = new();

    private readonly Dictionary<Type, IComponent> _components = new();
    private readonly List<IBehavior> _behaviors = new();
    private readonly List<IBehavior> _pendingAdd = new();
    private readonly List<IBehavior> _pendingRemove = new();

    public IReadOnlyList<IBehavior> Behaviors => _behaviors;

    public Entity Add<T>(T component) where T : class, IComponent
    {
        _components[typeof(T)] = component;
        return this;
    }

    public T Get<T>() where T : class, IComponent
        => _components.TryGetValue(typeof(T), out var c) ? (T)c
            : throw new InvalidOperationException($"Entity {Id} missing component {typeof(T).Name}");

    public T? TryGet<T>() where T : class, IComponent
        => _components.TryGetValue(typeof(T), out var c) ? (T)c : null;

    public bool Has<T>() where T : class, IComponent => _components.ContainsKey(typeof(T));

    public Entity AddBehavior(IBehavior behavior)
    {
        _pendingAdd.Add(behavior);
        return this;
    }

    public void RemoveBehavior(IBehavior behavior) => _pendingRemove.Add(behavior);

    public T? FindBehavior<T>() where T : class, IBehavior
    {
        foreach (var b in _behaviors) if (b is T t) return t;
        return null;
    }

    public void FlushBehaviorChanges(World world)
    {
        if (_pendingAdd.Count > 0)
        {
            foreach (var b in _pendingAdd)
            {
                _behaviors.Add(b);
                b.OnAttach(this, world);
            }
            _pendingAdd.Clear();
        }
        if (_pendingRemove.Count > 0)
        {
            foreach (var b in _pendingRemove)
            {
                if (_behaviors.Remove(b)) b.OnDetach(this, world);
            }
            _pendingRemove.Clear();
        }
    }

    public void Kill() => IsAlive = false;
}
