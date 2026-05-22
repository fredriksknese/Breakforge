using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Breakforge.Core;

namespace Breakforge.Definitions;

/// <summary>
/// Data definition of a brick "kind". Cheap to construct, immutable in spirit.
/// Behaviors are recreated per-brick via factories so each brick gets a clean
/// behavior instance (no shared state).
/// </summary>
public sealed class BrickDef
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public int Hp { get; init; } = 1;
    public int ScoreValue { get; init; } = 10;
    public Color Color { get; init; } = Color.SteelBlue;
    public Color? OutlineColor { get; init; }
    public Vector2 Size { get; init; } = new(48, 20);
    public float PowerupDropChance { get; init; } = 0.0f;
    public List<string>? PowerupPool { get; init; }  // ids of PowerupDef to pick from
    public bool Indestructible { get; init; }

    /// <summary>
    /// Behaviors to attach. Factory pattern so each brick instance gets its
    /// own behavior state (movement phase, cooldown timers, etc.).
    /// </summary>
    public List<Func<IBehavior>> BehaviorFactories { get; init; } = new();
}
