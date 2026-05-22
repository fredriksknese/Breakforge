using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Breakforge.Definitions;

namespace Breakforge.Content;

/// <summary>
/// Two starter maps. Add new ones by appending more LevelDefs + MapDefs.
/// Layout chars come from each level's Legend; "." = empty cell.
/// </summary>
public static class MapCatalog
{
    public static void Register(Registry r)
    {
        // ── map.tutorial ────────────────────────────────────────────
        r.Register(new LevelDef
        {
            Id = "tutorial.1",
            DisplayName = "First Strike",
            Legend = new() { ['B'] = "basic.blue", ['G'] = "basic.green", ['.'] = "" },
            Rows = new List<string>
            {
                "BBBBBBBBBB",
                "BGGGGGGGGB",
                "BG......GB",
                "BBBBBBBBBB",
            },
        });
        r.Register(new LevelDef
        {
            Id = "tutorial.2",
            DisplayName = "Bombs Away",
            Legend = new() { ['B'] = "basic.blue", ['#'] = "wall.gray", ['X'] = "bomb.red", ['.'] = "" },
            Rows = new List<string>
            {
                "##########",
                "#BBBBBBBB#",
                "#B..XX..B#",
                "#BBBBBBBB#",
                "..........",
            },
        });
        r.Register(new MapDef
        {
            Id = "map.tutorial",
            DisplayName = "Training Grounds",
            Subtitle = "Learn the basics",
            LevelIds = new() { "tutorial.1", "tutorial.2" },
        });

        // ── map.cascade ─────────────────────────────────────────────
        r.Register(new LevelDef
        {
            Id = "cascade.1",
            DisplayName = "Resonance",
            Legend = new()
            {
                ['B'] = "basic.blue", ['G'] = "basic.green", ['P'] = "basic.purple",
                ['A'] = "chain.amber", ['M'] = "mover.cyan", ['X'] = "bomb.red",
                ['#'] = "wall.gray", ['.'] = "",
            },
            BackgroundColor = new Color(20, 14, 30),
            Rows = new List<string>
            {
                "##########",
                "#PPPPPPPP#",
                "#AAGGGGAA#",
                "#..MMMM..#",
                "#GG.XX.GG#",
                "#BBBBBBBB#",
            },
        });
        r.Register(new MapDef
        {
            Id = "map.cascade",
            DisplayName = "Cascade Halls",
            Subtitle = "Chain reactions galore",
            LevelIds = new() { "cascade.1" },
        });

        // ── map.forge (locked by default; unlocked via "map.unlock.forge" powerup) ──
        r.Register(new LevelDef
        {
            Id = "forge.1",
            DisplayName = "The Anvil",
            Legend = new()
            {
                ['B'] = "basic.blue", ['G'] = "basic.green", ['P'] = "basic.purple",
                ['A'] = "chain.amber", ['M'] = "mover.cyan", ['X'] = "bomb.red",
                ['#'] = "wall.gray", ['.'] = "",
            },
            BackgroundColor = new Color(28, 18, 12),
            Rows = new List<string>
            {
                "##########",
                "#XPPPPPPX#",
                "#PAAAAAAP#",
                "#PA.MM.AP#",
                "#PAAAAAAP#",
                "#XPPPPPPX#",
            },
        });
        r.Register(new MapDef
        {
            Id = "map.forge",
            DisplayName = "The Forge",
            Subtitle = "Hidden — drops from purple bricks",
            LevelIds = new() { "forge.1" },
            LockedByDefault = true,
        });
    }
}
