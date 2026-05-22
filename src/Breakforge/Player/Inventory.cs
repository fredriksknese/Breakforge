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

    public void Add(string id, int qty = 1)
    {
        _counts.TryGetValue(id, out var n);
        _counts[id] = n + qty;
    }

    public bool TryConsume(string id)
    {
        if (!_counts.TryGetValue(id, out var n) || n <= 0) return false;
        _counts[id] = n - 1;
        if (_counts[id] == 0) _counts.Remove(id);
        return true;
    }

    public int Count(string id) => _counts.TryGetValue(id, out var n) ? n : 0;
}
