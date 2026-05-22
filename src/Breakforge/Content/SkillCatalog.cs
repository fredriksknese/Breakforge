using Breakforge.Definitions;

namespace Breakforge.Content;

/// <summary>
/// Demo skill tree: three branches (paddle, ball, economy). Each node has
/// a small OnLevelStart hook that nudges the run.
/// </summary>
public static class SkillCatalog
{
    public static void Register(Registry r)
    {
        r.Register(new SkillNode
        {
            Id = "paddle.wide.i",
            DisplayName = "Broad Stance I",
            Description = "+10% starting paddle width",
            Category = SkillCategory.Paddle,
            Cost = 1,
            OnLevelStart = ctx => ctx.PaddleWidthMultiplier *= 1.10f,
        });

        r.Register(new SkillNode
        {
            Id = "paddle.wide.ii",
            DisplayName = "Broad Stance II",
            Description = "+15% starting paddle width",
            Category = SkillCategory.Paddle,
            Cost = 2,
            Prerequisites = { "paddle.wide.i" },
            OnLevelStart = ctx => ctx.PaddleWidthMultiplier *= 1.15f,
        });

        r.Register(new SkillNode
        {
            Id = "ball.swift",
            DisplayName = "Swift Strike",
            Description = "+15% base ball speed",
            Category = SkillCategory.Ball,
            Cost = 1,
            OnLevelStart = ctx => ctx.BallSpeedMultiplier *= 1.15f,
        });

        r.Register(new SkillNode
        {
            Id = "ball.spare",
            DisplayName = "Spare Forge",
            Description = "Start every level with one extra ball ready",
            Category = SkillCategory.Ball,
            Cost = 2,
            OnLevelStart = ctx => ctx.BonusStartingBalls += 1,
        });

        r.Register(new SkillNode
        {
            Id = "map.forge.scout",
            DisplayName = "Forge Scout",
            Description = "Unlocks The Forge map without finding a key",
            Category = SkillCategory.Map,
            Cost = 3,
            OnLevelStart = ctx => ctx.Profile.UnlockMap("map.forge"),
        });

        r.Register(new SkillNode
        {
            Id = "econ.bargain",
            DisplayName = "Bargain Hunter",
            Description = "Powerup drops are slightly more common",
            Category = SkillCategory.Economy,
            Cost = 1,
            // Hook intentionally light — placeholder for a real drop-rate mod.
            OnLevelStart = _ => { /* hook for a future drop-rate modifier */ },
        });
    }
}
