using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Breakforge.Definitions;

/// <summary>
/// A single playable level layout. Rows of brick ids (or "." for empty).
/// The grid origin is top-left; cell size is derived from playfield + grid dims.
/// </summary>
public sealed class LevelDef
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }

    /// <summary>Each string is a row, each char (or space-separated token) is a brick id.</summary>
    public required List<string> Rows { get; init; }

    /// <summary>Map of single-char codes used in Rows to BrickDef ids.</summary>
    public required Dictionary<char, string> Legend { get; init; }

    public Color BackgroundColor { get; init; } = new Color(10, 12, 24);
    public Color WallColor { get; init; } = new Color(40, 44, 60);

    /// <summary>Optional override: a multiplier on all brick drop chances.</summary>
    public float PowerupDropMultiplier { get; init; } = 1f;
}
