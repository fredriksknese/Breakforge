using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Breakforge.Core;

/// <summary>
/// Code-generated drawing primitives. We avoid the Content Pipeline by drawing
/// everything from a single 1x1 white texture (rects, lines) and a pre-baked
/// circle texture. Swap to real sprites later by changing SpriteRenderer use.
/// </summary>
public static class Primitives
{
    public static Texture2D Pixel { get; private set; } = null!;
    private static Texture2D _circle = null!;
    private const int CircleTexSize = 128;

    public static void Init(GraphicsDevice gd)
    {
        Pixel = new Texture2D(gd, 1, 1);
        Pixel.SetData(new[] { Color.White });

        _circle = new Texture2D(gd, CircleTexSize, CircleTexSize);
        var data = new Color[CircleTexSize * CircleTexSize];
        float r = CircleTexSize * 0.5f;
        for (int y = 0; y < CircleTexSize; y++)
        for (int x = 0; x < CircleTexSize; x++)
        {
            float dx = x + 0.5f - r;
            float dy = y + 0.5f - r;
            float d = (float)System.Math.Sqrt(dx * dx + dy * dy);
            // 1px soft edge
            float a = Microsoft.Xna.Framework.MathHelper.Clamp(r - d, 0f, 1f);
            data[y * CircleTexSize + x] = new Color(1f, 1f, 1f, a);
        }
        _circle.SetData(data);
    }

    public static void FillRect(SpriteBatch sb, Rectangle r, Color c) => sb.Draw(Pixel, r, c);

    public static void FillRect(SpriteBatch sb, Vector2 pos, Vector2 size, Color c)
        => sb.Draw(Pixel, new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y), c);

    public static void HollowRect(SpriteBatch sb, Rectangle r, int thickness, Color c)
    {
        sb.Draw(Pixel, new Rectangle(r.X, r.Y, r.Width, thickness), c);
        sb.Draw(Pixel, new Rectangle(r.X, r.Y + r.Height - thickness, r.Width, thickness), c);
        sb.Draw(Pixel, new Rectangle(r.X, r.Y, thickness, r.Height), c);
        sb.Draw(Pixel, new Rectangle(r.X + r.Width - thickness, r.Y, thickness, r.Height), c);
    }

    public static void Line(SpriteBatch sb, Vector2 a, Vector2 b, Color c, float thickness = 1f)
    {
        var diff = b - a;
        float len = diff.Length();
        float ang = (float)System.Math.Atan2(diff.Y, diff.X);
        sb.Draw(Pixel, a, null, c, ang, Vector2.Zero,
            new Vector2(len, thickness), SpriteEffects.None, 0f);
    }

    public static void Circle(SpriteBatch sb, Vector2 center, float radius, Color c)
    {
        float scale = (radius * 2f) / CircleTexSize;
        sb.Draw(_circle, center, null, c, 0f,
            new Vector2(CircleTexSize * 0.5f, CircleTexSize * 0.5f),
            new Vector2(scale, scale), SpriteEffects.None, 0f);
    }
}
