using System.Collections.Generic;

namespace Breakforge.Core;

public enum BallElement
{
    Physical,
    Fire,   // applies burn DoT to the brick
    Ice,    // slows nearby movers + chance to stun
    Chaos,  // random extra damage roll on every hit
}

/// <summary>
/// Aggregate of every skill-driven modifier for the active run. Skills push
/// values into this in their OnLevelStart hooks; gameplay systems
/// (CollisionSystem, DamageResolver, PlayScene) read it.
///
/// One place to add new stats — keeps damage/economy/survivability decoupled
/// from individual skills.
/// </summary>
public sealed class PlayerStats
{
    // ── Offence ───────────────────────────────────────────────────
    public int BaseDamage = 1;
    public int FlatBonusDamage;
    public float DamageMultiplier = 1f;
    public float CritChance;
    public float CritMultiplier = 2f;
    public float ArmorPen;                // 0..1 — fraction of armor ignored
    public float OverkillSplashFraction;  // leftover damage spread to neighbours
    public float FirstStrikeMultiplier = 1f;
    public float RandomHitChance;         // chance an extra random brick is hit

    // ── Element ───────────────────────────────────────────────────
    public BallElement Element = BallElement.Physical;
    public float BurnDamagePerSecond = 2f;
    public float BurnDuration = 3f;
    public float ChaosBonusMax = 3f;      // chaos: extra damage rolled 0..max

    // ── Brick interactions ────────────────────────────────────────
    public float StunChance;
    public float StunDuration = 1.2f;
    public float SlowFactor = 1f;         // 1.0 = normal, 0.5 = half-speed movers
    public float KnockbackStrength;       // pixels of nudge per hit
    public float AreaMultiplier = 1f;     // extends explosive / chain radius
    public Dictionary<string, float> BrickDamageMultByDefId = new();

    // ── Speeds + dimensions ───────────────────────────────────────
    public float BallSpeedMultiplier = 1f;
    public float PaddleSpeedMultiplier = 1f;
    public float PaddleWidthMultiplier = 1f;
    public int BonusStartingBalls;

    // ── Survivability ─────────────────────────────────────────────
    public float LifeStealChance;         // per kill, chance to gain a life
    public int StartingShields;           // ball-saves on the paddle
    public float HealthRegenPerSecond;    // life points / sec (1.0 = +1 life/sec)
    public float LastStandMultiplier = 1f; // damage mult when lives==1
    public float CooldownReduction;       // shortens DoT period + regen period

    // ── Economy ───────────────────────────────────────────────────
    public int BonusGoldPerKill;
    public float GemDropChance;           // % chance a brick drops a gem (+score)
}

/// <summary>
/// Per-level mutable run state. Cleared per level. Holds anything that the
/// CollisionSystem / behaviors / scene need to share that isn't on an entity.
/// </summary>
public sealed class RunState
{
    public int Gold;
    public int Gems;
    public float HealthRegenAccumulator;
    public bool LastStandActive;
    /// <summary>Counts down. When 0 and another regen tick lands, +1 life.</summary>
    public float NextLifeRegenIn;
}
