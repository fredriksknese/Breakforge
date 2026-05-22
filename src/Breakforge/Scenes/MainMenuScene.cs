using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Breakforge.Core;

namespace Breakforge.Scenes;

public sealed class MainMenuScene : IScene
{
    private static readonly string[] Items = { "PLAY", "MAPS", "SKILLS", "INVENTORY", "QUIT" };
    private int _selected;
    private readonly KeyEdges _keys = new();

    public void Update(SceneContext ctx, GameTime gt)
    {
        if (_keys.Pressed(Keys.Down)) _selected = (_selected + 1) % Items.Length;
        if (_keys.Pressed(Keys.Up))   _selected = (_selected - 1 + Items.Length) % Items.Length;
        if (_keys.Pressed(Keys.Enter) || _keys.Pressed(Keys.Space))
        {
            switch (Items[_selected])
            {
                case "PLAY":      ctx.Scenes.Goto(new PlayScene(ctx.Registry.Map("map.tutorial"))); break;
                case "MAPS":      ctx.Scenes.Goto(new MapSelectScene()); break;
                case "SKILLS":    ctx.Scenes.Goto(new SkillTreeScene()); break;
                case "INVENTORY": ctx.Scenes.Goto(new InventoryScene()); break;
                case "QUIT":      System.Environment.Exit(0); break;
            }
        }
        _keys.EndFrame();
    }

    public void Draw(SceneContext ctx, SpriteBatch sb)
    {
        var vb = ctx.ViewportBounds;
        PixelFont.DrawCentered(sb, "BREAKFORGE", new Vector2(vb.Center.X, vb.Top + 80), Color.White, 6);
        PixelFont.DrawCentered(sb, "FORGE YOUR BUILD - BREAK THE BRICKS",
            new Vector2(vb.Center.X, vb.Top + 140), new Color(180, 180, 200), 2);

        int y = vb.Center.Y - (Items.Length * 28) / 2;
        for (int i = 0; i < Items.Length; i++)
        {
            var c = i == _selected ? Color.Gold : new Color(200, 200, 220);
            string prefix = i == _selected ? "> " : "  ";
            PixelFont.DrawCentered(sb, prefix + Items[i], new Vector2(vb.Center.X, y), c, 3);
            y += 32;
        }

        PixelFont.DrawCentered(sb, "ARROWS + ENTER",
            new Vector2(vb.Center.X, vb.Bottom - 36), new Color(120, 120, 150), 2);
    }
}
