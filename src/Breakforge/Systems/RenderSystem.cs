using Microsoft.Xna.Framework.Graphics;
using Breakforge.Core;

namespace Breakforge.Systems;

/// <summary>
/// Draws every SpriteRenderer either as a filled box or a circle, sorted by Layer.
/// Behaviors get OnDraw too (called after this), for overlays like the powerup label.
/// </summary>
public sealed class RenderSystem : ISystem
{
    public void Draw(World world, SpriteBatch sb)
    {
        // Sort by layer (lower drawn first). Allocate-free pass: we just iterate twice.
        // For modest entity counts a sorted list is fine; we use an in-place sort each frame.
        var snapshot = new System.Collections.Generic.List<Entity>(world.Entities.Count);
        foreach (var e in world.Entities)
            if (e.IsAlive && e.Has<SpriteRenderer>()) snapshot.Add(e);
        snapshot.Sort((a, b) => a.Get<SpriteRenderer>().Layer.CompareTo(b.Get<SpriteRenderer>().Layer));

        foreach (var e in snapshot)
        {
            var t = e.Get<Transform>();
            var s = e.Get<SpriteRenderer>();
            if (s.Circle)
            {
                Primitives.Circle(sb, t.Position, t.Size.X * 0.5f, s.Color);
            }
            else
            {
                Primitives.FillRect(sb, t.Bounds, s.Color);
                if (s.OutlineColor.HasValue)
                    Primitives.HollowRect(sb, t.Bounds, 1, s.OutlineColor.Value);
            }
        }
    }
}
