using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Breakforge.Core;
using Breakforge.Definitions;
using Breakforge.Gameplay;
using Breakforge.Systems;
using Breakforge.Effects;

namespace Breakforge.Scenes;

/// <summary>
/// The actual brick-breaker scene. Owns a World, a CollisionSystem subscription,
/// and the per-level setup. Re-creates the World on level transitions.
/// </summary>
public sealed class PlayScene : IScene
{
    private readonly MapDef _map;
    private int _levelIndex;

    private World _world = null!;
    private Entity _paddle = null!;
    private CollisionSystem _collision = null!;
    private readonly List<IDisposable> _subs = new();

    private int _score;
    private int _lives = 3;
    private float _flash; // tint when something dramatic happens
    private string _banner = "";
    private float _bannerTimer;

    private readonly KeyEdges _keys = new();

    public PlayScene(MapDef map) { _map = map; }

    public void Enter(SceneContext ctx)
    {
        BuildLevel(ctx);
    }

    private void BuildLevel(SceneContext ctx)
    {
        foreach (var s in _subs) s.Dispose();
        _subs.Clear();

        var vb = ctx.ViewportBounds;
        var playfield = new Rectangle(vb.Left + 32, vb.Top + 64, vb.Width - 64, vb.Height - 96);

        _world = new World { PlayField = playfield };
        var phys = new PhysicsSystem();
        _collision = new CollisionSystem();
        var render = new RenderSystem();
        var cleanup = new CleanupSystem();
        _world.Systems.Add(phys);
        _world.Systems.Add(_collision);
        _world.Systems.Add(render);
        _world.Systems.Add(cleanup);
        _collision.Attach(_world);

        // Run context: gather skill modifiers.
        var ctxRun = new RunContext
        {
            World = _world,
            Profile = ctx.Profile,
            Paddle = null!, // set below after paddle exists
        };

        // Paddle
        var paddleBaseSize = new Vector2(110f, 14f);
        _paddle = Spawning.SpawnPaddle(
            _world,
            new Vector2(playfield.Center.X, playfield.Bottom - 40),
            paddleBaseSize);
        ctxRun = new RunContext { World = _world, Profile = ctx.Profile, Paddle = _paddle };

        foreach (var skillId in ctx.Profile.Skills.Unlocked)
        {
            if (ctx.Registry.Skills.TryGetValue(skillId, out var node))
                node.OnLevelStart?.Invoke(ctxRun);
        }

        // Apply paddle width multiplier from skills.
        _paddle.Get<Transform>().Size.X *= ctxRun.PaddleWidthMultiplier;

        // Ball + extras from skill BonusStartingBalls.
        var firstBall = Spawning.SpawnStuckBall(_world, _paddle, Spawning.DefaultBallRadius);
        var launch = firstBall.FindBehavior<Behaviors.BallLaunchBehavior>();
        if (launch is not null && ctxRun.BallSpeedMultiplier != 1f)
        {
            // Replace launch with a scaled one.
            firstBall.RemoveBehavior(launch);
            firstBall.AddBehavior(new Behaviors.BallLaunchBehavior
            {
                Paddle = _paddle,
                LaunchSpeed = Spawning.DefaultBallSpeed * ctxRun.BallSpeedMultiplier,
            });
        }
        // BonusStartingBalls: spawn them stacked, ready to launch.
        for (int i = 0; i < ctxRun.BonusStartingBalls; i++)
            Spawning.SpawnStuckBall(_world, _paddle, Spawning.DefaultBallRadius);

        // Bricks
        var level = ctx.Registry.Level(_map.LevelIds[_levelIndex]);
        SpawnLevelBricks(ctx.Registry, level);

        // Event subscriptions
        _subs.Add(_world.Bus.Subscribe<BrickDestroyedEvent>(e => OnBrickDestroyed(ctx, e)));
        _subs.Add(_world.Bus.Subscribe<PowerupCollectedEvent>(e => OnPowerupCollected(ctx, e)));
        _subs.Add(_world.Bus.Subscribe<BallLostEvent>(e => OnBallLost(ctx, e)));

        ShowBanner("LEVEL " + (_levelIndex + 1) + " - " + level.DisplayName, 2.0f);
    }

    private void SpawnLevelBricks(Registry reg, LevelDef level)
    {
        var pf = _world.PlayField;
        int cols = 0;
        foreach (var row in level.Rows) if (row.Length > cols) cols = row.Length;
        int rows = level.Rows.Count;
        if (cols == 0 || rows == 0) return;

        float topPad = 16f;
        float sidePad = 16f;
        float cellW = (pf.Width - sidePad * 2) / cols;
        float cellH = 22f;

        for (int r = 0; r < rows; r++)
        {
            string row = level.Rows[r];
            for (int c = 0; c < row.Length; c++)
            {
                char ch = row[c];
                if (!level.Legend.TryGetValue(ch, out var brickId) || string.IsNullOrEmpty(brickId)) continue;
                if (!reg.Bricks.TryGetValue(brickId, out var def)) continue;
                var pos = new Vector2(
                    pf.Left + sidePad + cellW * (c + 0.5f),
                    pf.Top + topPad + cellH * (r + 0.5f));
                var brick = Spawning.SpawnBrick(_world, pos, def);
                // Fit brick to grid: snap size if not custom.
                var t = brick.Get<Transform>();
                t.Size = new Vector2(cellW - 4f, cellH - 4f);
            }
        }
    }

    private void OnBrickDestroyed(SceneContext ctx, BrickDestroyedEvent e)
    {
        var def = ctx.Registry.Brick(e.Brick.Get<DefRef>().DefId);
        _score += def.ScoreValue;
        ctx.Profile.LifetimeScore += def.ScoreValue;
        Spawning.SpawnFx(_world, e.Brick.Get<Transform>().Position, def.Color, 28f, 0.18f);

        // Powerup drop
        if (def.PowerupPool is { Count: > 0 } &&
            def.PowerupDropChance > 0 &&
            Rand.Chance(def.PowerupDropChance))
        {
            var puId = Rand.Pick(def.PowerupPool);
            if (ctx.Registry.Powerups.TryGetValue(puId, out var puDef))
                Spawning.SpawnPowerup(_world, e.Brick.Get<Transform>().Position, puDef);
        }

        // Win check (deferred a frame so chain reactions settle).
        if (!AnyBreakableBricksLeft())
        {
            _bannerTimer = 1.2f;
            _banner = "LEVEL CLEARED";
            ctx.Scenes.Goto(NextLevelScene(ctx));
        }
    }

    private bool AnyBreakableBricksLeft()
    {
        foreach (var e in _world.WithKind(EntityKind.Brick))
        {
            var h = e.TryGet<Health>();
            if (h is not null && !h.Invulnerable && h.Current > 0) return true;
        }
        return false;
    }

    private IScene NextLevelScene(SceneContext ctx)
    {
        if (_levelIndex + 1 < _map.LevelIds.Count)
        {
            _levelIndex++;
            BuildLevel(ctx);
            return this;
        }
        // Map cleared
        if (_map.CompletionReward is { } reward)
            ctx.Profile.Inventory.Add(reward);
        ctx.Profile.SkillPoints += 1;
        return new MainMenuScene();
    }

    private void OnPowerupCollected(SceneContext ctx, PowerupCollectedEvent e)
    {
        if (!ctx.Registry.Powerups.TryGetValue(e.PowerupId, out var def)) return;

        if (def.StoreInInventory)
        {
            ctx.Profile.Inventory.Add(def.Id);
            ShowBanner("BANKED " + def.DisplayName.ToUpper(), 1.5f);
            return;
        }

        // Create the effect; apply to the chosen target stack.
        var effect = def.EffectFactory();
        switch (def.Target)
        {
            case PowerupTarget.Paddle:
                effect.Target = _paddle;
                _world.GlobalEffects.Add(effect, _world);
                break;
            case PowerupTarget.AllBalls:
                // Re-apply per-ball — many of these effects (BallSizeEffect) bind to one target.
                foreach (var ball in _world.WithKind(EntityKind.Ball))
                {
                    var perBall = def.EffectFactory();
                    perBall.Target = ball;
                    _world.GlobalEffects.Add(perBall, _world);
                }
                break;
            case PowerupTarget.Global:
                _world.GlobalEffects.Add(effect, _world);
                break;
        }
        ShowBanner(def.DisplayName.ToUpper(), 1.0f);
    }

    private void OnBallLost(SceneContext ctx, BallLostEvent e)
    {
        // Only lose a life if no balls remain.
        bool anyBall = false;
        foreach (var b in _world.WithKind(EntityKind.Ball)) { anyBall = true; break; }
        if (anyBall) return;

        _lives -= 1;
        if (_lives <= 0)
        {
            ShowBanner("GAME OVER", 1.5f);
            ctx.Scenes.Goto(new MainMenuScene());
            return;
        }
        Spawning.SpawnStuckBall(_world, _paddle, Spawning.DefaultBallRadius);
    }

    private void ShowBanner(string text, float seconds)
    {
        _banner = text;
        _bannerTimer = seconds;
    }

    public void Update(SceneContext ctx, GameTime gt)
    {
        float dt = (float)gt.ElapsedGameTime.TotalSeconds;
        _world.Update(dt);
        if (_bannerTimer > 0) _bannerTimer -= dt;
        if (_flash > 0) _flash -= dt * 4f;

        if (_keys.Pressed(Keys.Escape)) ctx.Scenes.Goto(new MainMenuScene());
        _keys.EndFrame();
    }

    public void Draw(SceneContext ctx, SpriteBatch sb)
    {
        var vb = ctx.ViewportBounds;
        var levelDef = ctx.Registry.Level(_map.LevelIds[_levelIndex]);

        // Background + playfield frame
        Primitives.FillRect(sb, vb, levelDef.BackgroundColor);
        Primitives.HollowRect(sb, _world.PlayField, 2, levelDef.WallColor);

        // Entities
        _world.Draw(sb);

        // HUD
        DrawHud(sb, vb, levelDef);

        if (_bannerTimer > 0)
            PixelFont.DrawCentered(sb, _banner,
                new Vector2(vb.Center.X, vb.Center.Y - 40), Color.Gold, 5);
    }

    private void DrawHud(SpriteBatch sb, Rectangle vb, LevelDef level)
    {
        PixelFont.Draw(sb, "SCORE " + _score, new Vector2(vb.Left + 24, vb.Top + 16), Color.White, 3);
        PixelFont.Draw(sb, "LIVES " + _lives, new Vector2(vb.Right - 200, vb.Top + 16), Color.White, 3);
        PixelFont.Draw(sb, level.DisplayName.ToUpper(),
            new Vector2(vb.Center.X - PixelFont.MeasureWidth(level.DisplayName.ToUpper(), 2) / 2, vb.Top + 22), new Color(180, 180, 200), 2);

        // Active global effects ribbon
        int x = vb.Left + 24;
        int y = vb.Bottom - 24;
        foreach (var fx in _world.GlobalEffects.Active)
        {
            string label = ShortenEffectId(fx.Id) + " " + System.MathF.Ceiling(fx.RemainingSeconds);
            PixelFont.Draw(sb, label, new Vector2(x, y - 12), new Color(200, 220, 255), 2);
            x += PixelFont.MeasureWidth(label, 2) + 16;
        }
    }

    private static string ShortenEffectId(string id) => id.ToUpper();

    public void Exit()
    {
        foreach (var s in _subs) s.Dispose();
        _subs.Clear();
        _collision?.Detach();
    }
}
