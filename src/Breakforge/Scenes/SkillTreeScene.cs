using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Breakforge.Core;
using Breakforge.Definitions;

namespace Breakforge.Scenes;

public sealed class SkillTreeScene : IScene
{
    private List<SkillNode> _nodes = new();
    private int _selected;
    private readonly KeyEdges _keys = new();

    public void Enter(SceneContext ctx)
    {
        _nodes = ctx.Registry.Skills.Values
            .OrderBy(n => n.Category).ThenBy(n => n.Id).ToList();
    }

    public void Update(SceneContext ctx, GameTime gt)
    {
        if (_nodes.Count > 0)
        {
            if (_keys.Pressed(Keys.Down)) _selected = (_selected + 1) % _nodes.Count;
            if (_keys.Pressed(Keys.Up))   _selected = (_selected - 1 + _nodes.Count) % _nodes.Count;
            if (_keys.Pressed(Keys.Enter))
                ctx.Profile.Skills.Unlock(_nodes[_selected], ctx.Profile);
        }
        if (_keys.Pressed(Keys.Escape)) ctx.Scenes.Goto(new MainMenuScene());
        _keys.EndFrame();
    }

    public void Draw(SceneContext ctx, SpriteBatch sb)
    {
        var vb = ctx.ViewportBounds;
        PixelFont.DrawCentered(sb, "SKILL TREE", new Vector2(vb.Center.X, vb.Top + 50), Color.White, 4);
        PixelFont.DrawCentered(sb, "POINTS: " + ctx.Profile.SkillPoints,
            new Vector2(vb.Center.X, vb.Top + 90), Color.Gold, 3);

        int y = vb.Top + 140;
        var prof = ctx.Profile;
        for (int i = 0; i < _nodes.Count; i++)
        {
            var n = _nodes[i];
            bool owned = prof.Skills.Has(n.Id);
            bool can = prof.Skills.CanUnlock(n, prof);
            Color c = owned ? new Color(120, 220, 140) : can ? new Color(220, 220, 220) : new Color(120, 120, 140);
            if (i == _selected) c = Color.Gold;
            string prefix = i == _selected ? "> " : "  ";
            string status = owned ? "[OWNED]" : "[" + n.Cost + " PT]";
            PixelFont.Draw(sb, prefix + n.DisplayName.ToUpper() + " " + status,
                new Vector2(vb.Left + 80, y), c, 2);
            PixelFont.Draw(sb, n.Description.ToUpper(),
                new Vector2(vb.Left + 110, y + 16), new Color(150, 150, 170), 2);
            y += 40;
        }

        PixelFont.DrawCentered(sb, "ESC: BACK   ENTER: UNLOCK",
            new Vector2(vb.Center.X, vb.Bottom - 36), new Color(120, 120, 150), 2);
    }
}
