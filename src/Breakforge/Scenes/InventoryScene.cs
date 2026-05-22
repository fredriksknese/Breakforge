using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Breakforge.Core;

namespace Breakforge.Scenes;

public sealed class InventoryScene : IScene
{
    private readonly KeyEdges _keys = new();

    public void Update(SceneContext ctx, GameTime gt)
    {
        if (_keys.Pressed(Keys.Escape)) ctx.Scenes.Goto(new MainMenuScene());
        _keys.EndFrame();
    }

    public void Draw(SceneContext ctx, SpriteBatch sb)
    {
        var vb = ctx.ViewportBounds;
        PixelFont.DrawCentered(sb, "INVENTORY", new Vector2(vb.Center.X, vb.Top + 60), Color.White, 5);
        int y = vb.Top + 140;
        var items = ctx.Profile.Inventory.Items.OrderBy(kv => kv.Key).ToList();
        if (items.Count == 0)
        {
            PixelFont.DrawCentered(sb, "EMPTY", new Vector2(vb.Center.X, y), new Color(140, 140, 170), 3);
        }
        else
        {
            foreach (var (id, n) in items)
            {
                if (!ctx.Registry.Powerups.TryGetValue(id, out var p)) continue;
                PixelFont.Draw(sb, "X" + n + "  " + p.DisplayName.ToUpper(),
                    new Vector2(vb.Center.X - 200, y), new Color(220, 220, 220), 3);
                y += 32;
            }
        }
        PixelFont.DrawCentered(sb, "ESC: BACK",
            new Vector2(vb.Center.X, vb.Bottom - 36), new Color(120, 120, 150), 2);
    }
}
