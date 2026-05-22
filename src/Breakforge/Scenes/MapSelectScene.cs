using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Breakforge.Core;
using Breakforge.Definitions;

namespace Breakforge.Scenes;

public sealed class MapSelectScene : IScene
{
    private List<MapDef> _maps = new();
    private int _selected;
    private readonly KeyEdges _keys = new();

    public void Enter(SceneContext ctx)
    {
        _maps = ctx.Registry.Maps.Values
            .Where(m => !m.LockedByDefault || ctx.Profile.IsMapUnlocked(m.Id))
            .OrderBy(m => m.Id).ToList();
    }

    public void Update(SceneContext ctx, GameTime gt)
    {
        if (_maps.Count == 0) { if (_keys.Pressed(Keys.Escape)) ctx.Scenes.Goto(new MainMenuScene()); _keys.EndFrame(); return; }
        if (_keys.Pressed(Keys.Down)) _selected = (_selected + 1) % _maps.Count;
        if (_keys.Pressed(Keys.Up))   _selected = (_selected - 1 + _maps.Count) % _maps.Count;
        if (_keys.Pressed(Keys.Enter)) ctx.Scenes.Goto(new PlayScene(_maps[_selected]));
        if (_keys.Pressed(Keys.Escape)) ctx.Scenes.Goto(new MainMenuScene());
        _keys.EndFrame();
    }

    public void Draw(SceneContext ctx, SpriteBatch sb)
    {
        var vb = ctx.ViewportBounds;
        PixelFont.DrawCentered(sb, "MAPS", new Vector2(vb.Center.X, vb.Top + 60), Color.White, 5);
        int y = vb.Top + 140;
        for (int i = 0; i < _maps.Count; i++)
        {
            var m = _maps[i];
            var c = i == _selected ? Color.Gold : new Color(200, 200, 220);
            string prefix = i == _selected ? "> " : "  ";
            PixelFont.DrawCentered(sb, prefix + m.DisplayName.ToUpper(), new Vector2(vb.Center.X, y), c, 3);
            PixelFont.DrawCentered(sb, m.Subtitle.ToUpper(),
                new Vector2(vb.Center.X, y + 22), new Color(140, 140, 170), 2);
            y += 56;
        }

        // Show locked maps as teasers.
        int hidden = ctx.Registry.Maps.Values.Count(m => m.LockedByDefault && !ctx.Profile.IsMapUnlocked(m.Id));
        if (hidden > 0)
            PixelFont.DrawCentered(sb, "+ " + hidden + " LOCKED",
                new Vector2(vb.Center.X, y + 20), new Color(120, 120, 150), 2);

        PixelFont.DrawCentered(sb, "ESC: BACK   ENTER: PLAY",
            new Vector2(vb.Center.X, vb.Bottom - 36), new Color(120, 120, 150), 2);
    }
}
