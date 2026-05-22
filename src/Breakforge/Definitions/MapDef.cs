using System.Collections.Generic;

namespace Breakforge.Definitions;

/// <summary>
/// A map is an ordered run of levels with shared theme. "Unique maps" unlock
/// via skill tree or powerup pickup (see PlayerProfile.UnlockedMaps).
/// </summary>
public sealed class MapDef
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Subtitle { get; init; }
    public required List<string> LevelIds { get; init; }

    /// <summary>If true, this map is hidden until unlocked.</summary>
    public bool LockedByDefault { get; init; }

    /// <summary>If set, the listed powerup id awarded when clearing the final level.</summary>
    public string? CompletionReward { get; init; }
}
