using System.Collections.Generic;

namespace Breakforge.Definitions;

/// <summary>
/// Central catalog of all data definitions. Populated once at startup by the
/// content catalogs; consumed by gameplay code. Adding new content = adding
/// to a catalog, not editing engine code.
/// </summary>
public sealed class Registry
{
    public Dictionary<string, BrickDef> Bricks { get; } = new();
    public Dictionary<string, PowerupDef> Powerups { get; } = new();
    public Dictionary<string, LevelDef> Levels { get; } = new();
    public Dictionary<string, MapDef> Maps { get; } = new();
    public Dictionary<string, SkillNode> Skills { get; } = new();

    public BrickDef Brick(string id) => Bricks[id];
    public PowerupDef Powerup(string id) => Powerups[id];
    public LevelDef Level(string id) => Levels[id];
    public MapDef Map(string id) => Maps[id];
    public SkillNode Skill(string id) => Skills[id];

    public void Register(BrickDef b) => Bricks[b.Id] = b;
    public void Register(PowerupDef p) => Powerups[p.Id] = p;
    public void Register(LevelDef l) => Levels[l.Id] = l;
    public void Register(MapDef m) => Maps[m.Id] = m;
    public void Register(SkillNode s) => Skills[s.Id] = s;
}
