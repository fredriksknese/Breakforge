using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Breakforge.Behaviors;
using Breakforge.Definitions;

namespace Breakforge.Content;

/// <summary>
/// Built-in brick library. Add a new brick: append one entry here.
/// </summary>
public static class BrickCatalog
{
    public static void Register(Registry r)
    {
        r.Register(new BrickDef
        {
            Id = "basic.blue",
            DisplayName = "Common Brick",
            Hp = 1, ScoreValue = 10,
            Color = new Color(70, 130, 200),
            PowerupDropChance = 0.06f,
            PowerupPool = new List<string> { "paddle.wider", "ball.size.big", "ball.split" },
        });

        r.Register(new BrickDef
        {
            Id = "basic.green",
            DisplayName = "Tough Brick",
            Hp = 2, ScoreValue = 25,
            Color = new Color(90, 170, 110),
            OutlineColor = new Color(40, 100, 50),
            PowerupDropChance = 0.10f,
            PowerupPool = new List<string> { "ball.multi", "ball.explosive" },
        });

        r.Register(new BrickDef
        {
            Id = "basic.purple",
            DisplayName = "Hardened Brick",
            Hp = 3, ScoreValue = 50,
            Color = new Color(140, 90, 200),
            OutlineColor = new Color(80, 50, 120),
            PowerupDropChance = 0.15f,
            PowerupPool = new List<string> { "ball.explosive", "map.unlock.forge" },
        });

        r.Register(new BrickDef
        {
            Id = "wall.gray",
            DisplayName = "Stone Wall",
            Hp = 99, Indestructible = true,
            Color = new Color(70, 72, 84),
            OutlineColor = new Color(120, 122, 134),
            ScoreValue = 0,
        });

        r.Register(new BrickDef
        {
            Id = "bomb.red",
            DisplayName = "Bomb",
            Hp = 1,
            Color = new Color(220, 70, 70),
            OutlineColor = new Color(255, 200, 200),
            ScoreValue = 30,
            PowerupDropChance = 0.20f,
            PowerupPool = new List<string> { "ball.explosive" },
            BehaviorFactories = new List<System.Func<Core.IBehavior>>
            {
                () => new BrickAreaDamageOnDeathBehavior { Radius = 96f, Damage = 99 }
            }
        });

        r.Register(new BrickDef
        {
            Id = "chain.amber",
            DisplayName = "Resonant Brick",
            Hp = 2,
            Color = new Color(220, 160, 60),
            OutlineColor = new Color(255, 220, 140),
            ScoreValue = 35,
            BehaviorFactories = new List<System.Func<Core.IBehavior>>
            {
                () => new BrickDamagesNeighborsBehavior { Radius = 90f, Damage = 1, MaxTargets = 2 }
            }
        });

        r.Register(new BrickDef
        {
            Id = "mover.cyan",
            DisplayName = "Drifter",
            Hp = 1,
            Color = new Color(80, 200, 220),
            OutlineColor = new Color(160, 240, 255),
            ScoreValue = 20,
            PowerupDropChance = 0.12f,
            PowerupPool = new List<string> { "ball.split" },
            BehaviorFactories = new List<System.Func<Core.IBehavior>>
            {
                () => new BrickHorizontalMoverBehavior { Speed = 60f, HalfRange = 70f }
            }
        });
    }
}
