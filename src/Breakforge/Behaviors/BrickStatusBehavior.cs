using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakforge.Core;
using Breakforge.Gameplay;

namespace Breakforge.Behaviors;

/// <summary>
/// Drives the BrickStatus component each tick: burn damage over time,
/// stun/slow timers decay, knockback nudge decays. Drawn last so the
/// brick's renderer can be shifted by the current knock offset and tinted
/// by active statuses.
/// </summary>
public sealed class BrickStatusBehavior : IBehavior
{
    private float _burnAccum;

    public void OnUpdate(Entity self, World world, float dt)
    {
        var s = self.TryGet<BrickStatus>();
        if (s is null) return;

        if (s.BurnRemaining > 0f)
        {
            s.BurnRemaining -= dt;
            _burnAccum += s.BurnDps * dt;
            if (_burnAccum >= 1f)
            {
                int ticks = (int)_burnAccum;
                _burnAccum -= ticks;
                DamageResolver.HitBrickArea(world, source: null, self, ticks);
            }
            if (s.BurnRemaining <= 0f) { s.BurnDps = 0f; _burnAccum = 0f; }
        }

        if (s.StunRemaining > 0f) s.StunRemaining -= dt;

        if (s.SlowRemaining > 0f)
        {
            s.SlowRemaining -= dt;
            if (s.SlowRemaining <= 0f) s.SlowFactor = 1f;
        }

        // Knockback decays toward zero each frame.
        if (s.KnockOffset.LengthSquared() > 0.01f)
            s.KnockOffset *= System.MathF.Max(0f, 1f - 8f * dt);
        else
            s.KnockOffset = Vector2.Zero;
    }

    public void OnDraw(Entity self, World world, SpriteBatch sb)
    {
        var s = self.TryGet<BrickStatus>();
        if (s is null) return;
        // Visual overlays: burn ember, ice halo, stun ring, knockback ghost.
        var t = self.Get<Transform>();
        var center = t.Position + s.KnockOffset;
        if (s.BurnRemaining > 0f)
            Primitives.Circle(sb, center, t.Size.X * 0.35f, new Color(255, 120, 60, 90));
        if (s.SlowRemaining > 0f && s.SlowFactor < 1f)
            Primitives.HollowRect(sb, t.Bounds, 1, new Color(120, 200, 255, 180));
        if (s.IsStunned)
            Primitives.Circle(sb, new Vector2(center.X, t.Bounds.Top - 4), 3, Color.Gold);
    }
}
