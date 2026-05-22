using System;
using System.Collections.Generic;

namespace Breakforge.Core;

public interface IGameEvent { }

/// <summary>
/// Synchronous in-process pub/sub. Powerup behaviors, scoring, FX, and chain
/// reactions all subscribe here instead of poking each other directly.
/// </summary>
public sealed class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent
    {
        if (!_handlers.TryGetValue(typeof(T), out var list))
            _handlers[typeof(T)] = list = new List<Delegate>();
        list.Add(handler);
        return new Subscription(() => list.Remove(handler));
    }

    public void Publish<T>(T evt) where T : IGameEvent
    {
        if (!_handlers.TryGetValue(typeof(T), out var list)) return;
        // Copy to avoid mutation during iteration (handlers may subscribe/unsubscribe).
        var snapshot = list.ToArray();
        foreach (var d in snapshot) ((Action<T>)d)(evt);
    }

    private sealed class Subscription : IDisposable
    {
        private Action? _dispose;
        public Subscription(Action dispose) { _dispose = dispose; }
        public void Dispose() { _dispose?.Invoke(); _dispose = null; }
    }
}
