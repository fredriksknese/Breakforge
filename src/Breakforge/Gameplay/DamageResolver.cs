using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Breakforge.Core;
using Breakforge.Definitions;

namespace Breakforge.Gameplay;

/// <summary>
/// Single pipeline that turns a ball/brick collision into damage + side
/// effects (crit, armor, element, knockback, stun, gold/gem drops,
/// life-steal, overkill splash, random hit). Centralised so adding a new
/// skill is "set a PlayerStats field" rather than "patch CollisionSystem".
/// </summary>
public static class DamageResolver
{
    /// <summary>Resolve a normal ball-on-brick hit.</summary>
    public static void HitBrick(World world, Entity ball, Entity brick, Registry? registry = null)
    {
        var stats = world.Stats;
        var run = world.Run;
        var h = brick.TryGet<Health>();
        if (h is null || h.Invulnerable) return;

        int dmg = ComputeDamage(world, brick, isAreaSplash: false, out bool crit);
        ApplyDamage(world, ball, brick, dmg, crit, allowSecondaryEffects: true, registry);
    }

    /// <summary>Resolve area damage (explosions, chains) without secondary effects.</summary>
    public static void HitBrickArea(World world, Entity? source, Entity brick, int rawAmount, Registry? registry = null)
    {
        var h = brick.TryGet<Health>();
        if (h is null || h.Invulnerable) return;
        int dmg = ApplyArmor(rawAmount, h, world.Stats);
        ApplyDamageRaw(world, source, brick, dmg, crit: false, allowSecondaryEffects: false, registry);
    }

    private static int ComputeDamage(World world, Entity brick, bool isAreaSplash, out bool crit)
    {
        var stats = world.Stats;
        crit = false;
        float dmg = stats.BaseDamage + stats.FlatBonusDamage;
        dmg *= stats.DamageMultiplier;

        // First strike: bonus on the very first damage instance this brick takes.
        var h = brick.Get<Health>();
        if (!h.FirstHitTaken) dmg *= stats.FirstStrikeMultiplier;

        // Last stand: amplification when down to last life.
        if (world.Run.LastStandActive) dmg *= stats.LastStandMultiplier;

        // Brick-type bonus (e.g. +30% vs Armored).
        var defRef = brick.TryGet<DefRef>();
        if (defRef is not null
            && stats.BrickDamageMultByDefId.TryGetValue(defRef.DefId, out var typeMult))
            dmg *= typeMult;

        // Chaos element: random extra damage on every hit.
        if (stats.Element == BallElement.Chaos)
            dmg += Rand.RangeF(0f, stats.ChaosBonusMax);

        // Crit roll (no double-crits on area splashes — feels spammy).
        if (!isAreaSplash && stats.CritChance > 0f && Rand.Chance(stats.CritChance))
        {
            crit = true;
            dmg *= stats.CritMultiplier;
        }

        dmg = ApplyArmor((int)System.MathF.Ceiling(dmg), h, stats);
        return System.Math.Max(1, (int)dmg);
    }

    private static int ApplyArmor(int dmg, Health h, PlayerStats stats)
    {
        if (h.Armor <= 0) return dmg;
        int effectiveArmor = (int)System.MathF.Round(h.Armor * (1f - System.Math.Clamp(stats.ArmorPen, 0f, 1f)));
        return System.Math.Max(1, dmg - effectiveArmor);
    }

    private static void ApplyDamage(World world, Entity ball, Entity brick, int dmg, bool crit,
        bool allowSecondaryEffects, Registry? registry)
        => ApplyDamageRaw(world, ball, brick, dmg, crit, allowSecondaryEffects, registry);

    private static void ApplyDamageRaw(World world, Entity? source, Entity brick, int dmg, bool crit,
        bool allowSecondaryEffects, Registry? registry)
    {
        var stats = world.Stats;
        var h = brick.Get<Health>();
        int hpBefore = h.Current;
        h.Current -= dmg;
        h.FirstHitTaken = true;
        world.Bus.Publish(new BrickDamagedEvent(brick, dmg, source));
        if (crit) world.Bus.Publish(new BrickCritEvent(brick, dmg));

        if (allowSecondaryEffects)
        {
            ApplyElementSideEffects(world, brick);
            ApplyKnockback(world, source, brick);
            ApplyStun(world, brick);
        }

        if (h.Current <= 0)
        {
            // Overkill — leftover splashes to nearby bricks.
            int leftover = -h.Current;
            brick.Kill();
            world.Bus.Publish(new BrickDestroyedEvent(brick, source));
            if (allowSecondaryEffects && leftover > 0 && stats.OverkillSplashFraction > 0f)
            {
                int splash = System.Math.Max(1, (int)(leftover * stats.OverkillSplashFraction));
                world.Bus.Publish(new BrickOverkillEvent(brick, splash, source));
                world.Bus.Publish(new AreaDamageEvent(
                    brick.Get<Transform>().Position,
                    70f * stats.AreaMultiplier,
                    splash, source));
            }

            // Life steal.
            if (stats.LifeStealChance > 0f && Rand.Chance(stats.LifeStealChance))
                world.Bus.Publish(new LifeStolenEvent());

            // Gold + gem economy.
            int gold = stats.BonusGoldPerKill;
            if (gold > 0)
            {
                world.Run.Gold += gold;
                world.Bus.Publish(new GoldGainedEvent(gold, brick.Get<Transform>().Position));
            }
            if (stats.GemDropChance > 0f && Rand.Chance(stats.GemDropChance))
            {
                world.Run.Gems += 1;
                world.Bus.Publish(new GemDroppedEvent(brick, 50));
            }
        }

        if (allowSecondaryEffects)
        {
            // Random hit: spread to another random alive brick at base damage.
            if (stats.RandomHitChance > 0f && Rand.Chance(stats.RandomHitChance))
            {
                var alt = PickRandomOtherBrick(world, brick);
                if (alt is not null)
                {
                    int side = System.Math.Max(1, (int)(dmg * 0.5f));
                    HitBrickArea(world, source, alt, side, registry);
                    Spawning.SpawnFx(world, alt.Get<Transform>().Position, Color.MediumPurple, 18f, 0.18f);
                }
            }
        }
    }

    private static Entity? PickRandomOtherBrick(World world, Entity exclude)
    {
        var pool = new List<Entity>();
        foreach (var b in world.WithKind(EntityKind.Brick))
        {
            if (ReferenceEquals(b, exclude)) continue;
            var hh = b.TryGet<Health>();
            if (hh is null || hh.Invulnerable || hh.Current <= 0) continue;
            pool.Add(b);
        }
        return pool.Count == 0 ? null : pool[Rand.Range(0, pool.Count)];
    }

    private static void ApplyElementSideEffects(World world, Entity brick)
    {
        var stats = world.Stats;
        var status = brick.TryGet<BrickStatus>();
        if (status is null) return;

        float cdr = System.Math.Clamp(stats.CooldownReduction, 0f, 0.8f);
        switch (stats.Element)
        {
            case BallElement.Fire:
                status.BurnRemaining = System.Math.Max(status.BurnRemaining, stats.BurnDuration * (1f + cdr));
                status.BurnDps = System.Math.Max(status.BurnDps, stats.BurnDamagePerSecond);
                break;
            case BallElement.Ice:
                status.SlowFactor = System.Math.Min(status.SlowFactor, 0.35f);
                status.SlowRemaining = 2.5f * (1f + cdr);
                if (Rand.Chance(0.2f))
                    status.StunRemaining = System.Math.Max(status.StunRemaining, 0.6f);
                break;
        }
    }

    private static void ApplyStun(World world, Entity brick)
    {
        var stats = world.Stats;
        if (stats.StunChance <= 0f) return;
        if (!Rand.Chance(stats.StunChance)) return;
        var status = brick.TryGet<BrickStatus>();
        if (status is null) return;
        status.StunRemaining = System.Math.Max(status.StunRemaining, stats.StunDuration);
    }

    private static void ApplyKnockback(World world, Entity? source, Entity brick)
    {
        var stats = world.Stats;
        if (stats.KnockbackStrength <= 0f || source is null) return;
        var status = brick.TryGet<BrickStatus>();
        if (status is null) return;
        var dir = brick.Get<Transform>().Position - source.Get<Transform>().Position;
        if (dir.LengthSquared() < 0.001f) dir = new Vector2(0, -1);
        dir.Normalize();
        status.KnockOffset += dir * stats.KnockbackStrength;
    }
}
