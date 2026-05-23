using Breakforge.Core;
using Breakforge.Definitions;

namespace Breakforge.Content;

/// <summary>
/// Skill tree definitions. Each node nudges PlayerStats during OnLevelStart;
/// CollisionSystem + DamageResolver + PlayScene read those stats at runtime.
/// Add a new skill = one Register call here. Costs / prereqs balance build
/// pacing; tune freely.
/// </summary>
public static class SkillCatalog
{
    public static void Register(Registry r)
    {
        // ── Paddle ──────────────────────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "paddle.wide.i",
            DisplayName = "Broad Stance I",
            Description = "+10% starting paddle width",
            Category = SkillCategory.Paddle,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.PaddleWidthMultiplier *= 1.10f,
        });
        r.Register(new SkillNode
        {
            Id = "paddle.wide.ii",
            DisplayName = "Broad Stance II",
            Description = "+15% starting paddle width",
            Category = SkillCategory.Paddle,
            Cost = 2,
            Prerequisites = { "paddle.wide.i" },
            OnLevelStart = ctx => ctx.Stats.PaddleWidthMultiplier *= 1.15f,
        });
        r.Register(new SkillNode
        {
            Id = "paddle.speed",
            DisplayName = "Quickstep",
            Description = "+25% paddle top speed",
            Category = SkillCategory.Paddle,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.PaddleSpeedMultiplier *= 1.25f,
        });

        // ── Ball ────────────────────────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "ball.swift",
            DisplayName = "Swift Strike",
            Description = "+15% base ball speed",
            Category = SkillCategory.Ball,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.BallSpeedMultiplier *= 1.15f,
        });
        r.Register(new SkillNode
        {
            Id = "ball.precise",
            DisplayName = "Steady Hand",
            Description = "-10% ball speed, +5% crit chance",
            Category = SkillCategory.Ball,
            Cost = 1,
            OnLevelStart = ctx =>
            {
                ctx.Stats.BallSpeedMultiplier *= 0.90f;
                ctx.Stats.CritChance += 0.05f;
            },
        });
        r.Register(new SkillNode
        {
            Id = "ball.spare",
            DisplayName = "Spare Forge",
            Description = "Start every level with one extra ball ready",
            Category = SkillCategory.Ball,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.BonusStartingBalls += 1,
        });

        // ── Offense ─────────────────────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "offense.damage.flat",
            DisplayName = "Raw Power",
            Description = "+1 flat damage on every hit",
            Category = SkillCategory.Offense,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.FlatBonusDamage += 1,
        });
        r.Register(new SkillNode
        {
            Id = "offense.damage.pct",
            DisplayName = "Honed Edge",
            Description = "+25% damage to bricks",
            Category = SkillCategory.Offense,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.DamageMultiplier *= 1.25f,
        });
        r.Register(new SkillNode
        {
            Id = "offense.crit.chance",
            DisplayName = "Sharp Eye",
            Description = "+10% critical strike chance",
            Category = SkillCategory.Offense,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.CritChance += 0.10f,
        });
        r.Register(new SkillNode
        {
            Id = "offense.crit.damage",
            DisplayName = "Brutal Crits",
            Description = "Critical strikes deal +75% damage (×3.5 total)",
            Category = SkillCategory.Offense,
            Cost = 2,
            Prerequisites = { "offense.crit.chance" },
            OnLevelStart = ctx => ctx.Stats.CritMultiplier += 1.5f,
        });
        r.Register(new SkillNode
        {
            Id = "offense.armorpen",
            DisplayName = "Piercing Strike",
            Description = "Ignore 50% of brick armor",
            Category = SkillCategory.Offense,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.ArmorPen = System.Math.Max(ctx.Stats.ArmorPen, 0.50f),
        });
        r.Register(new SkillNode
        {
            Id = "offense.firststrike",
            DisplayName = "First Strike",
            Description = "First hit on each brick deals ×1.6 damage",
            Category = SkillCategory.Offense,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.FirstStrikeMultiplier *= 1.6f,
        });
        r.Register(new SkillNode
        {
            Id = "offense.randomhit",
            DisplayName = "Ricochet",
            Description = "15% chance: hit splashes half-damage to a random brick",
            Category = SkillCategory.Offense,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.RandomHitChance += 0.15f,
        });
        r.Register(new SkillNode
        {
            Id = "offense.overkill",
            DisplayName = "Overkill",
            Description = "Excess damage splashes to nearby bricks",
            Category = SkillCategory.Offense,
            Cost = 3,
            OnLevelStart = ctx => ctx.Stats.OverkillSplashFraction = System.Math.Max(ctx.Stats.OverkillSplashFraction, 0.50f),
        });

        // ── Defense ─────────────────────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "defense.lifesteal",
            DisplayName = "Vampire",
            Description = "5% chance to gain a life on brick kill",
            Category = SkillCategory.Defense,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.LifeStealChance += 0.05f,
        });
        r.Register(new SkillNode
        {
            Id = "defense.shield",
            DisplayName = "Aegis",
            Description = "Start with a paddle shield that catches one lost ball",
            Category = SkillCategory.Defense,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.StartingShields += 1,
        });
        r.Register(new SkillNode
        {
            Id = "defense.regen",
            DisplayName = "Vitality",
            Description = "Regenerate +1 life every 5 seconds",
            Category = SkillCategory.Defense,
            Cost = 3,
            OnLevelStart = ctx => ctx.Stats.HealthRegenPerSecond += 0.20f,
        });
        r.Register(new SkillNode
        {
            Id = "defense.laststand",
            DisplayName = "Last Stand",
            Description = "×1.6 damage when down to your last life",
            Category = SkillCategory.Defense,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.LastStandMultiplier *= 1.6f,
        });

        // ── Element (pick one path) ─────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "element.fire",
            DisplayName = "Pyromancer",
            Description = "Fire element: hits burn bricks for 2 dmg/s over 3s",
            Category = SkillCategory.Element,
            Cost = 3,
            OnLevelStart = ctx => ctx.Stats.Element = BallElement.Fire,
        });
        r.Register(new SkillNode
        {
            Id = "element.ice",
            DisplayName = "Cryomancer",
            Description = "Ice element: hits slow bricks and may briefly stun",
            Category = SkillCategory.Element,
            Cost = 3,
            OnLevelStart = ctx => ctx.Stats.Element = BallElement.Ice,
        });
        r.Register(new SkillNode
        {
            Id = "element.chaos",
            DisplayName = "Chaos Touch",
            Description = "Chaos element: every hit rolls +0-3 bonus damage",
            Category = SkillCategory.Element,
            Cost = 3,
            OnLevelStart = ctx => ctx.Stats.Element = BallElement.Chaos,
        });

        // ── Status / Control ────────────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "status.stun",
            DisplayName = "Concussion",
            Description = "10% chance to stun struck bricks for 1.2s",
            Category = SkillCategory.Status,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.StunChance += 0.10f,
        });
        r.Register(new SkillNode
        {
            Id = "status.slow",
            DisplayName = "Glacial Field",
            Description = "Mover bricks move at 70% speed globally",
            Category = SkillCategory.Status,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.SlowFactor = System.Math.Min(ctx.Stats.SlowFactor, 0.70f),
        });
        r.Register(new SkillNode
        {
            Id = "status.knockback",
            DisplayName = "Impact",
            Description = "Hits visibly knock bricks around (+6 px nudge)",
            Category = SkillCategory.Status,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.KnockbackStrength += 6f,
        });

        // ── Utility ─────────────────────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "util.aoe",
            DisplayName = "Wider Blast",
            Description = "+30% explosion / chain radius",
            Category = SkillCategory.Ball,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.AreaMultiplier *= 1.30f,
        });
        r.Register(new SkillNode
        {
            Id = "util.cdr",
            DisplayName = "Tempo",
            Description = "+20% regen / burn DoT speed",
            Category = SkillCategory.Ball,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.CooldownReduction += 0.20f,
        });

        // ── Economy ─────────────────────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "econ.bargain",
            DisplayName = "Bargain Hunter",
            Description = "+1 gold per brick destroyed",
            Category = SkillCategory.Economy,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.BonusGoldPerKill += 1,
        });
        r.Register(new SkillNode
        {
            Id = "econ.gold",
            DisplayName = "Treasure Hunter",
            Description = "+2 additional gold per brick destroyed",
            Category = SkillCategory.Economy,
            Cost = 2,
            Prerequisites = { "econ.bargain" },
            OnLevelStart = ctx => ctx.Stats.BonusGoldPerKill += 2,
        });
        r.Register(new SkillNode
        {
            Id = "econ.gems",
            DisplayName = "Gemcrafter",
            Description = "5% chance a destroyed brick drops a gem (+50 score)",
            Category = SkillCategory.Economy,
            Cost = 2,
            OnLevelStart = ctx => ctx.Stats.GemDropChance += 0.05f,
        });

        // ── Brick-type bonuses ──────────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "vs.armored",
            DisplayName = "Wallbreaker",
            Description = "+50% damage to Hardened bricks",
            Category = SkillCategory.Brick,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.BrickDamageMultByDefId["basic.purple"] = 1.50f,
        });
        r.Register(new SkillNode
        {
            Id = "vs.tough",
            DisplayName = "Bruiser",
            Description = "+30% damage to Tough bricks",
            Category = SkillCategory.Brick,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.BrickDamageMultByDefId["basic.green"] = 1.30f,
        });
        r.Register(new SkillNode
        {
            Id = "vs.explosive",
            DisplayName = "Demolitionist",
            Description = "+50% damage to Bomb bricks (chain harder)",
            Category = SkillCategory.Brick,
            Cost = 1,
            OnLevelStart = ctx => ctx.Stats.BrickDamageMultByDefId["bomb.red"] = 1.50f,
        });

        // ── Map (existing) ──────────────────────────────────────────
        r.Register(new SkillNode
        {
            Id = "map.forge.scout",
            DisplayName = "Forge Scout",
            Description = "Unlocks The Forge map without finding a key",
            Category = SkillCategory.Map,
            Cost = 3,
            OnLevelStart = ctx => ctx.Profile.UnlockMap("map.forge"),
        });
    }
}
