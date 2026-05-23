using System.Collections.Generic;

namespace Breakforge.Player;

/// <summary>
/// All persistent player state: inventory, unlocked content, skill points,
/// lifetime score. Mutations flip <see cref="IsDirty"/> so the host loop can
/// auto-save without each call site doing it manually.
/// </summary>
public sealed class PlayerProfile
{
    private string _name = "Forger";
    private int _lifetimeScore;
    private int _skillPoints = 3; // starter points for the demo

    public PlayerProfile()
    {
        Inventory.Changed += MarkDirty;
        Skills.Changed += MarkDirty;
    }

    public string Name
    {
        get => _name;
        set { if (_name == value) return; _name = value; MarkDirty(); }
    }

    public Inventory Inventory { get; } = new();
    public SkillTreeState Skills { get; } = new();

    public HashSet<string> UnlockedMaps { get; } = new();

    public int LifetimeScore
    {
        get => _lifetimeScore;
        set { if (_lifetimeScore == value) return; _lifetimeScore = value; MarkDirty(); }
    }

    public int SkillPoints
    {
        get => _skillPoints;
        set { if (_skillPoints == value) return; _skillPoints = value; MarkDirty(); }
    }

    public bool IsMapUnlocked(string mapId) => UnlockedMaps.Contains(mapId);

    public void UnlockMap(string mapId)
    {
        if (UnlockedMaps.Add(mapId)) MarkDirty();
    }

    /// <summary>True when something has changed since the last <see cref="ClearDirty"/>.</summary>
    public bool IsDirty { get; private set; }

    public void MarkDirty() => IsDirty = true;
    public void ClearDirty() => IsDirty = false;
}
