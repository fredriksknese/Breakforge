using System;
using Microsoft.Xna.Framework;
using Breakforge.Core;

namespace Breakforge.Systems;

/// <summary>
/// Handles every collision interaction:
///   ball <-> playfield walls (bounce)
///   ball <-> paddle           (bounce + steering based on hit X)
///   ball <-> brick            (bounce + damage + event)
///   powerup <-> paddle        (pickup -> event)
///   AreaDamageEvent           (chain reaction damage application)
///
/// Resolves the swept ball using an axis-aligned shortest-overlap approach.
/// Fast enough for &lt; a few hundred bricks; swap to a grid later if needed.
/// </summary>
public sealed class CollisionSystem : ISystem
{
    private IDisposable? _areaSub;
    private World? _world;

    public void Attach(World world)
    {
        _world = world;
        _areaSub = world.Bus.Subscribe<AreaDamageEvent>(OnAreaDamage);
    }

    public void Detach()
    {
        _areaSub?.Dispose();
        _areaSub = null;
        _world = null;
    }

    public void Update(World world, float dt)
    {
        if (_world is null) Attach(world);

        // Ball vs walls + paddle + bricks
        foreach (var ball in world.WithKind(EntityKind.Ball))
        {
            var bt = ball.Get<Transform>();
            var bc = ball.Get<CircleCollider>();
            var bv = ball.Get<Velocity>();

            // Walls
            var field = world.PlayField;
            if (bt.Position.X - bc.Radius < field.Left)
            {
                bt.Position.X = field.Left + bc.Radius;
                bv.Value.X = MathF.Abs(bv.Value.X);
                world.Bus.Publish(new BallHitWallEvent(ball, new Vector2(1, 0)));
            }
            else if (bt.Position.X + bc.Radius > field.Right)
            {
                bt.Position.X = field.Right - bc.Radius;
                bv.Value.X = -MathF.Abs(bv.Value.X);
                world.Bus.Publish(new BallHitWallEvent(ball, new Vector2(-1, 0)));
            }
            if (bt.Position.Y - bc.Radius < field.Top)
            {
                bt.Position.Y = field.Top + bc.Radius;
                bv.Value.Y = MathF.Abs(bv.Value.Y);
                world.Bus.Publish(new BallHitWallEvent(ball, new Vector2(0, 1)));
            }
            else if (bt.Position.Y - bc.Radius > field.Bottom)
            {
                // Lost ball
                ball.Kill();
                world.Bus.Publish(new BallLostEvent(ball));
                continue;
            }

            // Paddle
            foreach (var paddle in world.WithKind(EntityKind.Paddle))
            {
                if (BallVsBox(ball, paddle, out var normal))
                {
                    // Steering: ball X velocity depends on hit offset from paddle center.
                    var pt = paddle.Get<Transform>();
                    float offset = MathHelper.Clamp((bt.Position.X - pt.Position.X) / (pt.Size.X * 0.5f), -1f, 1f);
                    float speed = bv.Value.Length();
                    var dir = new Vector2(offset * 0.75f, -1f);
                    dir.Normalize();
                    bv.Value = dir * speed;
                    world.Bus.Publish(new BallHitPaddleEvent(ball, paddle));
                }
            }

            // Bricks
            foreach (var brick in world.WithKind(EntityKind.Brick))
            {
                if (!brick.IsAlive) continue;
                if (BallVsBox(ball, brick, out var normal))
                {
                    // Reflect velocity off the contact normal.
                    if (normal.X != 0) bv.Value.X = -bv.Value.X;
                    if (normal.Y != 0) bv.Value.Y = -bv.Value.Y;

                    var h = brick.TryGet<Health>();
                    if (h is not null && !h.Invulnerable)
                    {
                        h.Current -= 1;
                        world.Bus.Publish(new BrickDamagedEvent(brick, 1, ball));
                        if (h.Current <= 0)
                        {
                            brick.Kill();
                            world.Bus.Publish(new BrickDestroyedEvent(brick, ball));
                        }
                    }
                    world.Bus.Publish(new BallHitBrickEvent(ball, brick, normal));
                    break; // one brick per ball per frame keeps physics stable
                }
            }
        }

        // Powerup vs paddle
        foreach (var pu in world.WithKind(EntityKind.Powerup))
        {
            if (!pu.IsAlive) continue;
            foreach (var paddle in world.WithKind(EntityKind.Paddle))
            {
                if (BoxVsBox(pu, paddle))
                {
                    var defId = pu.Get<DefRef>().DefId;
                    pu.Kill();
                    world.Bus.Publish(new PowerupCollectedEvent(pu, paddle, defId));
                    break;
                }
            }
        }
    }

    private void OnAreaDamage(AreaDamageEvent e)
    {
        if (_world is null) return;
        foreach (var brick in _world.WithKind(EntityKind.Brick))
        {
            if (!brick.IsAlive) continue;
            var pos = brick.Get<Transform>().Position;
            if (Vector2.DistanceSquared(pos, e.Center) > e.Radius * e.Radius) continue;
            var h = brick.TryGet<Health>();
            if (h is null || h.Invulnerable) continue;
            h.Current -= e.Amount;
            _world.Bus.Publish(new BrickDamagedEvent(brick, e.Amount, e.Source));
            if (h.Current <= 0)
            {
                brick.Kill();
                _world.Bus.Publish(new BrickDestroyedEvent(brick, e.Source));
            }
        }
    }

    private static bool BallVsBox(Entity ball, Entity box, out Vector2 normal)
    {
        var bt = ball.Get<Transform>();
        var bc = ball.Get<CircleCollider>();
        var r = box.Get<Transform>().Bounds;
        float cx = MathHelper.Clamp(bt.Position.X, r.Left, r.Right);
        float cy = MathHelper.Clamp(bt.Position.Y, r.Top, r.Bottom);
        float dx = bt.Position.X - cx;
        float dy = bt.Position.Y - cy;
        float d2 = dx * dx + dy * dy;
        if (d2 >= bc.Radius * bc.Radius) { normal = Vector2.Zero; return false; }

        // Pick the dominant axis to reflect on.
        // If the ball center is inside the box, push out along nearest edge.
        if (d2 < 0.0001f)
        {
            float leftDist = bt.Position.X - r.Left;
            float rightDist = r.Right - bt.Position.X;
            float topDist = bt.Position.Y - r.Top;
            float botDist = r.Bottom - bt.Position.Y;
            float minH = MathF.Min(leftDist, rightDist);
            float minV = MathF.Min(topDist, botDist);
            if (minH < minV) normal = leftDist < rightDist ? new Vector2(-1, 0) : new Vector2(1, 0);
            else             normal = topDist  < botDist  ? new Vector2(0, -1) : new Vector2(0, 1);
            return true;
        }
        normal = MathF.Abs(dx) > MathF.Abs(dy)
            ? new Vector2(MathF.Sign(dx), 0)
            : new Vector2(0, MathF.Sign(dy));
        // Push ball out of the box.
        float dist = MathF.Sqrt(d2);
        float push = bc.Radius - dist;
        bt.Position += new Vector2(dx, dy) / dist * push;
        return true;
    }

    private static bool BoxVsBox(Entity a, Entity b)
        => a.Get<Transform>().Bounds.Intersects(b.Get<Transform>().Bounds);
}
