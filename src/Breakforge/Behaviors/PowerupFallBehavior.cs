using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Breakforge.Core;

namespace Breakforge.Behaviors;

/// <summary>
/// Powerup capsule: drifts down, dies if it leaves the playfield.
/// Draws its short label on top of the capsule.
/// </summary>
public sealed class PowerupFallBehavior : IBehavior
{
    public string Label { get; init; } = "?";

    public void OnUpdate(Entity self, World world, float dt)
    {
        var t = self.Get<Transform>();
        // PhysicsSystem handles position; we just kill if off-field.
        if (t.Position.Y - t.Size.Y * 0.5f > world.PlayField.Bottom) self.Kill();
    }

    public void OnDraw(Entity self, World world, SpriteBatch sb)
    {
        var t = self.Get<Transform>();
        // Draw label centered, dark on top of bright capsule.
        PixelFont.DrawCentered(sb, Label, t.Position, Color.Black, 2);
    }
}
