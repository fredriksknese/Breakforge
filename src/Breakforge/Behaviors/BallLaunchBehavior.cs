using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Breakforge.Core;

namespace Breakforge.Behaviors;

/// <summary>
/// Ball sits on the paddle until the player presses Space / clicks. Detaches
/// itself after launching.
/// </summary>
public sealed class BallLaunchBehavior : IBehavior
{
    public Entity? Paddle { get; init; }
    public float LaunchSpeed { get; init; } = 360f;

    private MouseState _prevMouse;

    public void OnUpdate(Entity self, World world, float dt)
    {
        if (Paddle is null || !Paddle.IsAlive) { self.RemoveBehavior(this); return; }
        var pt = Paddle.Get<Transform>();
        var st = self.Get<Transform>();
        var sr = self.Get<CircleCollider>();
        st.Position = new Vector2(pt.Position.X, pt.Position.Y - pt.Size.Y * 0.5f - sr.Radius - 1f);

        var kb = Keyboard.GetState();
        var ms = Mouse.GetState();
        bool clicked = ms.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
        _prevMouse = ms;
        if (kb.IsKeyDown(Keys.Space) || clicked)
        {
            var v = self.Get<Velocity>();
            // Launch up + slight angle based on paddle velocity (skill expression).
            var pv = Paddle.Get<Velocity>();
            var dir = new Vector2(MathHelper.Clamp(pv.Value.X * 0.0015f, -0.6f, 0.6f), -1f);
            dir.Normalize();
            v.Value = dir * LaunchSpeed;
            self.RemoveBehavior(this);
        }
    }
}
