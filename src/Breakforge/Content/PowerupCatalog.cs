using Microsoft.Xna.Framework;
using Breakforge.Definitions;
using Breakforge.Effects;

namespace Breakforge.Content;

/// <summary>
/// Built-in powerups. Each one's EffectFactory builds a fresh effect each
/// pickup (effects carry per-instance state). Add a new powerup: one entry here.
/// </summary>
public static class PowerupCatalog
{
    public static void Register(Registry r)
    {
        r.Register(new PowerupDef
        {
            Id = "paddle.wider",
            DisplayName = "Wider Paddle",
            ShortLabel = "W",
            Color = new Color(120, 220, 140),
            Target = PowerupTarget.Paddle,
            EffectFactory = () => new PaddleWidthEffect { Factor = 1.6f, RemainingSeconds = 12f },
        });

        r.Register(new PowerupDef
        {
            Id = "ball.size.big",
            DisplayName = "Bigger Ball",
            ShortLabel = "B",
            Color = new Color(255, 220, 110),
            Target = PowerupTarget.AllBalls,
            EffectFactory = () => new BallSizeEffect { Factor = 1.7f, RemainingSeconds = 12f },
        });

        r.Register(new PowerupDef
        {
            Id = "ball.multi",
            DisplayName = "Multiball",
            ShortLabel = "M",
            Color = new Color(220, 120, 220),
            Target = PowerupTarget.Global,
            EffectFactory = () => new MultiBallEffect { ExtraBalls = 2 },
        });

        r.Register(new PowerupDef
        {
            Id = "ball.explosive",
            DisplayName = "Explosive Balls",
            ShortLabel = "X",
            Color = new Color(255, 110, 70),
            Target = PowerupTarget.Global,
            EffectFactory = () => new ExplosiveBallEffect { Radius = 70f, Damage = 1, RemainingSeconds = 10f },
        });

        r.Register(new PowerupDef
        {
            Id = "ball.split",
            DisplayName = "Splitter (next hit)",
            ShortLabel = "S",
            Color = new Color(120, 220, 220),
            Target = PowerupTarget.AllBalls,
            // Attaches a per-ball behavior via a wrapping effect: we cheat here
            // by treating it as a one-shot global effect that attaches behaviors.
            EffectFactory = () => new SplitterPowerupEffect { ExtraPerBall = 2 },
        });

        // Special: a powerup that unlocks a map. Demonstrates "powerups include maps".
        r.Register(new PowerupDef
        {
            Id = "map.unlock.forge",
            DisplayName = "Forge Key",
            ShortLabel = "K",
            Color = new Color(255, 230, 120),
            StoreInInventory = true,        // not consumed at pickup; banked
            Target = PowerupTarget.Global,
            EffectFactory = () => new MapUnlockEffect { MapId = "map.forge" },
        });
    }
}

/// <summary>
/// One-shot effect: attaches BallSplitOnFirstHitBehavior to every existing ball.
/// Defined here (close to its registration) for proximity; could move to Effects/.
/// </summary>
public sealed class SplitterPowerupEffect : Effect
{
    public override string Id => "ball.split.onhit";
    public int ExtraPerBall { get; init; } = 2;

    public override void OnApply(Core.World world)
    {
        foreach (var ball in world.WithKind(Core.EntityKind.Ball))
            ball.AddBehavior(new Behaviors.BallSplitOnFirstHitBehavior { ExtraBalls = ExtraPerBall });
        RemainingSeconds = 0.01f;
    }
}

/// <summary>One-shot effect: unlock a map on the active profile.</summary>
public sealed class MapUnlockEffect : Effect
{
    public override string Id => "meta.unlock-map";
    public required string MapId { get; init; }

    public override void OnApply(Core.World world)
    {
        if (BreakforgeGame.ActiveProfile is not null)
            BreakforgeGame.ActiveProfile.UnlockMap(MapId);
        RemainingSeconds = 0.01f;
    }
}
