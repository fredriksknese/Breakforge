using Microsoft.Xna.Framework;
using Breakforge.Core;
using Breakforge.Definitions;
using Breakforge.Behaviors;

namespace Breakforge.Gameplay;

/// <summary>
/// One-stop builder for the standard entities. Putting this here keeps spawn
/// logic out of scenes/effects/behaviors so they all stay decoupled.
/// </summary>
public static class Spawning
{
    public const float DefaultBallRadius = 7f;
    public const float DefaultBallSpeed = 360f;

    public static Entity SpawnPaddle(World world, Vector2 position, Vector2 size)
    {
        var e = new Entity { Kind = EntityKind.Paddle };
        e.Add(new Transform { Position = position, Size = size });
        e.Add(new Velocity { MaxSpeed = 900f });
        e.Add(new BoxCollider());
        e.Add(new SpriteRenderer { Color = new Color(220, 230, 255), Layer = 5 });
        e.AddBehavior(new PaddleInputBehavior());
        world.Spawn(e);
        return e;
    }

    public static Entity SpawnBall(World world, Vector2 position, Vector2 velocity, float radius)
    {
        var e = new Entity { Kind = EntityKind.Ball };
        e.Add(new Transform { Position = position, Size = new(radius * 2f, radius * 2f) });
        e.Add(new Velocity { Value = velocity, MaxSpeed = 1200f });
        e.Add(new CircleCollider { Radius = radius });
        e.Add(new SpriteRenderer { Color = Color.White, Layer = 8, Circle = true });
        world.Spawn(e);
        return e;
    }

    public static Entity SpawnStuckBall(World world, Entity paddle, float radius)
    {
        var pt = paddle.Get<Transform>();
        var pos = new Vector2(pt.Position.X, pt.Position.Y - pt.Size.Y * 0.5f - radius - 1f);
        var ball = SpawnBall(world, pos, Vector2.Zero, radius);
        ball.AddBehavior(new BallLaunchBehavior { Paddle = paddle, LaunchSpeed = DefaultBallSpeed });
        return ball;
    }

    public static Entity SpawnBrick(World world, Vector2 position, BrickDef def)
    {
        var e = new Entity { Kind = EntityKind.Brick };
        e.Add(new Transform { Position = position, Size = def.Size });
        e.Add(new BoxCollider());
        e.Add(new SpriteRenderer { Color = def.Color, OutlineColor = def.OutlineColor, Layer = 3 });
        e.Add(new Health { Max = def.Hp, Current = def.Hp, Armor = def.Armor, Invulnerable = def.Indestructible });
        e.Add(new DefRef { DefId = def.Id });
        e.Add(new BrickStatus());
        e.AddBehavior(new Behaviors.BrickStatusBehavior());
        foreach (var factory in def.BehaviorFactories) e.AddBehavior(factory());
        world.Spawn(e);
        return e;
    }

    public static Entity SpawnPowerup(World world, Vector2 position, PowerupDef def)
    {
        var e = new Entity { Kind = EntityKind.Powerup };
        e.Add(new Transform { Position = position, Size = new(28, 16) });
        e.Add(new Velocity { Value = new(0, 140f) });
        e.Add(new BoxCollider());
        e.Add(new SpriteRenderer { Color = def.Color, OutlineColor = Color.White, Layer = 6 });
        e.Add(new DefRef { DefId = def.Id });
        e.AddBehavior(new PowerupFallBehavior { Label = def.ShortLabel });
        world.Spawn(e);
        return e;
    }

    public static Entity SpawnFx(World world, Vector2 position, Color color, float radius, float life)
    {
        var e = new Entity { Kind = EntityKind.Fx };
        e.Add(new Transform { Position = position, Size = new(radius * 2f, radius * 2f) });
        e.Add(new SpriteRenderer { Color = color, Layer = 9, Circle = true });
        e.Add(new Lifetime { Remaining = life });
        world.Spawn(e);
        return e;
    }
}
