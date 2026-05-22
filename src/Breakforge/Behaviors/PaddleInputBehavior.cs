using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Breakforge.Core;

namespace Breakforge.Behaviors;

/// <summary>
/// Moves the paddle. Uses keyboard (A/D, arrow keys) and mouse X if available.
/// Designed to be replaceable: swap in a different behavior for AI / replay.
/// </summary>
public sealed class PaddleInputBehavior : IBehavior
{
    public float Acceleration { get; init; } = 6000f;
    public float Friction { get; init; } = 12f;
    public bool UseMouse { get; init; } = true;

    public void OnUpdate(Entity self, World world, float dt)
    {
        var t = self.Get<Transform>();
        var v = self.Get<Velocity>();
        var kb = Keyboard.GetState();

        float ax = 0;
        if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A)) ax -= 1;
        if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D)) ax += 1;

        if (UseMouse)
        {
            var ms = Mouse.GetState();
            // Soft mouse follow: target the mouse X and apply velocity toward it.
            float dx = ms.X - t.Position.X;
            if (System.MathF.Abs(dx) > 1f) ax = System.MathF.Sign(dx) * System.Math.Min(System.MathF.Abs(dx) / 80f, 1f);
        }

        v.Value.X += ax * Acceleration * dt;
        v.Value.X *= System.MathF.Max(0f, 1f - Friction * dt);
        v.Value.X = MathHelper.Clamp(v.Value.X, -v.MaxSpeed, v.MaxSpeed);
        // PhysicsSystem applies position. Post-clamp paddle to playfield here.
        float halfW = t.Size.X * 0.5f;
        var field = world.PlayField;
        if (t.Position.X - halfW < field.Left) { t.Position.X = field.Left + halfW; v.Value.X = 0; }
        if (t.Position.X + halfW > field.Right) { t.Position.X = field.Right - halfW; v.Value.X = 0; }
    }
}
