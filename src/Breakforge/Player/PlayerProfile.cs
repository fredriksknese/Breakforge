using System.Collections.Generic;

namespace Breakforge.Player;

/// <summary>
/// All persistent player state: inventory, unlocked content, skill points,
/// lifetime score. Kept simple and serializable.
/// </summary>
public sealed class PlayerProfile
{
    public string Name { get; set; } = "Forger";
    public Inventory Inventory { get; } = new();
    public SkillTreeState Skills { get; } = new();

    public HashSet<string> UnlockedMaps { get; } = new();

    public int LifetimeScore { get; set; }
    public int SkillPoints { get; set; } = 3; // starter points for the demo

    public bool IsMapUnlocked(string mapId) => UnlockedMaps.Contains(mapId);
    public void UnlockMap(string mapId) => UnlockedMaps.Add(mapId);
}
