using System;
using System.Collections.Generic;

namespace Breakforge.Player;

/// <summary>
/// Bag of powerup ids → count. Stored powerups can be activated before a level
/// starts (via InventoryScene). Pickup powerups that have StoreInInventory=true
/// flow here.
/// </summary>
public sealed class Inventory
{
    private readonly Dictionary<string, int> _counts = new();
    public IReadOnlyDictionary<string, int> Items => _counts;

    /// <summary>Fired whenever contents change. Used by PlayerProfile for auto-save.</summary>
    public event Action? Changed;

    public void Add(string id, int qty = 1)
    {
        _counts.TryGetValue(id, out var n);
        _counts[id] = n + qty;
        Changed?.Invoke();
    }

    public bool TryConsume(string id)
    {
        if (!_counts.TryGetValue(id, out var n) || n <= 0) return false;
        _counts[id] = n - 1;
        if (_counts[id] == 0) _counts.Remove(id);
        Changed?.Invoke();
        return true;
    }

    public int Count(string id) => _counts.TryGetValue(id, out var n) ? n : 0;

    /// <summary>Replace contents from a saved snapshot without raising Changed.</summary>
    internal void LoadFrom(IReadOnlyDictionary<string, int> items)
    {
        _counts.Clear();
        foreach (var kv in items)
            if (kv.Value > 0) _counts[kv.Key] = kv.Value;
    }
}
