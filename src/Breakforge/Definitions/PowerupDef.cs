using System;
using Microsoft.Xna.Framework;
using Breakforge.Core;
using Breakforge.Effects;

namespace Breakforge.Definitions;

public enum PowerupTarget { Global, Paddle, AllBalls }

/// <summary>
/// Definition of a collectible powerup that drops from bricks. When picked up
/// by the paddle, it asks its factory for one or more effects and registers
/// them with the world or a target entity.
/// </summary>
public sealed class PowerupDef
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public Color Color { get; init; } = Color.Gold;
    public string ShortLabel { get; init; } = "?"; // 1-3 letters drawn on the capsule
    public PowerupTarget Target { get; init; } = PowerupTarget.Paddle;
    public required Func<IEffect> EffectFactory { get; init; }

    /// <summary>If true, also stored in inventory on pickup instead of being consumed.</summary>
    public bool StoreInInventory { get; init; }
}
