using System;
using System.Collections.Generic;
using Breakforge.Core;

namespace Breakforge.Definitions;

public enum SkillCategory { Paddle, Ball, Brick, Map, Economy, Offense, Defense, Element, Status }

/// <summary>
/// A skill tree node. Each node has prerequisites and an OnUnlock hook that
/// mutates the world setup (initial paddle size, base ball speed, starting
/// inventory items, unlocked maps, etc.).
/// </summary>
public sealed class SkillNode
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public SkillCategory Category { get; init; } = SkillCategory.Paddle;
    public int Cost { get; init; } = 1;
    public List<string> Prerequisites { get; init; } = new();

    /// <summary>Applied to the run when starting a new level.</summary>
    public Action<RunContext>? OnLevelStart { get; init; }
}

/// <summary>Context passed to skill OnLevelStart hooks.</summary>
public sealed class RunContext
{
    public required World World { get; init; }
    public required Entity Paddle { get; init; }
    public required Player.PlayerProfile Profile { get; init; }
    /// <summary>All stat modifiers go through here. Skills mutate fields directly.</summary>
    public PlayerStats Stats { get; init; } = new();
}
