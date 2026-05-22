# Breakforge

**Forge your build, break the bricks.** A brick-breaker with a skill tree, an
inventory, modifiable balls and paddles, chain-reaction bricks, and unique
unlockable maps. Built in C# on [MonoGame](https://www.monogame.net/)
(DesktopGL — cross-platform).

This repo is an extendable foundation, not a finished game. The systems are
in place; the content is small on purpose so the patterns stay obvious.

---

## Run

Requires the .NET 10 SDK (or .NET 8/9 — adjust `TargetFramework` in
`src/Breakforge/Breakforge.csproj`).

```bash
dotnet run --project src/Breakforge
```

Controls:

| Action            | Key                |
|-------------------|--------------------|
| Move paddle       | A/D, Arrows, Mouse |
| Launch ball       | Space / Left Click |
| Menu navigation   | Arrows + Enter     |
| Back              | Esc                |

---

## Architecture at a glance

The goal is *composition over inheritance*. There is no `Brick` class, no
`Ball` class, no `ExplosiveBall` subclass. Everything is an `Entity` with a
bag of **components** (data) and a list of **behaviors** (logic). New gameplay
ideas plug in by writing a small behavior or effect — never by editing the
engine.

```
                 ┌────────────────┐
                 │ BreakforgeGame │  Game shell (MonoGame Game)
                 └────────┬───────┘
                          │ owns
                          ▼
                  ┌──────────────┐
                  │ SceneManager │  Menu / Play / Skills / Maps / Inventory
                  └───────┬──────┘
                          │
                  ┌───────▼──────┐
                  │  PlayScene   │ owns one World per session
                  └───────┬──────┘
                          │
                  ┌───────▼──────┐
                  │    World     │  Entities + Systems + EventBus + EffectStack
                  └──────────────┘
```

### Layers

| Folder           | Role                                                                  |
|------------------|-----------------------------------------------------------------------|
| `Core/`          | Entity, Component, Behavior, World, EventBus, drawing primitives.     |
| `Systems/`       | Cross-cutting frame logic: physics, collision, render, cleanup.       |
| `Behaviors/`     | Pluggable per-entity logic (paddle input, ball split, brick mover…).  |
| `Effects/`       | Time-bound modifiers applied by powerups (wider paddle, bigger ball). |
| `Definitions/`   | Data classes: `BrickDef`, `PowerupDef`, `LevelDef`, `MapDef`, `SkillNode`. |
| `Content/`       | Built-in catalogs that register defs into the `Registry`.             |
| `Gameplay/`      | `Spawning` — factory functions that build the standard entities.      |
| `Player/`        | `PlayerProfile`, `Inventory`, `SkillTreeState`.                       |
| `Scenes/`        | Menu, MapSelect, SkillTree, Inventory, Play.                          |

### The five core concepts

1. **Entity + Components.** An `Entity` is just `Id + Kind + Tags +
   Dictionary<Type, IComponent>`. Components are pure data: `Transform`,
   `Velocity`, `CircleCollider`, `Health`, `SpriteRenderer`, etc.

2. **Behaviors.** An `IBehavior` has `OnAttach`, `OnDetach`, `OnUpdate`,
   `OnDraw`. Each frame, the `World` ticks every entity's behaviors. Behaviors
   are how features compose: a brick can be "moving + explosive on death +
   damages neighbors when hit" simply by holding three behaviors.

3. **EventBus.** Synchronous pub/sub. Chain reactions, scoring, drop logic,
   FX, and cross-cutting concerns subscribe here instead of poking each other
   directly. Events are records (`BrickDestroyedEvent`, `BallHitBrickEvent`,
   `AreaDamageEvent`, etc.).

4. **EffectStack.** Time-bounded modifiers. Powerups translate into one or
   more `IEffect`s applied to a target (paddle / ball / world). The stack
   ticks each effect, removes dead ones, and runs their `OnApply` /
   `OnRemove` hooks so the world stays in a consistent state.

5. **Registry + Definitions.** All content (bricks, powerups, levels, maps,
   skill nodes) is described by POCO definitions in `Definitions/`. Catalogs
   in `Content/` register them at startup. Switch to JSON / YAML later by
   replacing the catalogs — gameplay code reads only the `Registry`.

---

## Extending — recipes

### Add a new brick kind

In `Content/BrickCatalog.cs`:

```csharp
r.Register(new BrickDef {
    Id          = "ice.cyan",
    DisplayName = "Frozen Brick",
    Hp          = 2,
    Color       = new Color(180, 220, 255),
    BehaviorFactories = { () => new MyFreezeBehavior() }, // optional
});
```

Then use the brick's id in any level's `Legend`.

### Add a new powerup

Define an effect (or reuse one) and register the powerup:

```csharp
r.Register(new PowerupDef {
    Id            = "ball.fast",
    DisplayName   = "Quick Ball",
    ShortLabel    = "Q",
    Color         = Color.LightSkyBlue,
    Target        = PowerupTarget.AllBalls,
    EffectFactory = () => new MyBallSpeedEffect { Factor = 1.4f, RemainingSeconds = 8f },
});
```

Reference it from a brick's `PowerupPool`.

### Add a new effect

```csharp
public sealed class MyEffect : Effect {
    public override string Id => "ball.speed";
    public float Factor { get; init; } = 1.4f;
    private Vector2 _saved;

    public override void OnApply(World w)  { /* mutate target */ }
    public override void OnRemove(World w) { /* restore */ }
}
```

### Add a new level

Append to `Content/MapCatalog.cs` with a layout of single-char codes:

```csharp
r.Register(new LevelDef {
    Id      = "cascade.2",
    Legend  = new() { ['B'] = "basic.blue", ['X'] = "bomb.red", ['.'] = "" },
    Rows    = new List<string> {
        "BBBBBBBBBB",
        "B........B",
        "B..XXXX..B",
        "BBBBBBBBBB",
    },
});
```

### Add a new map (and gate it behind a powerup)

```csharp
r.Register(new MapDef {
    Id              = "map.crucible",
    DisplayName     = "Crucible",
    Subtitle        = "Late-game challenge",
    LevelIds        = new() { "crucible.1", "crucible.2" },
    LockedByDefault = true,
});
```

Drop a powerup with `StoreInInventory = true` that calls
`profile.UnlockMap("map.crucible")` via its effect (see
`MapUnlockEffect` in `PowerupCatalog.cs` for a working example).

### Add a skill node

```csharp
r.Register(new SkillNode {
    Id            = "ball.spare.ii",
    DisplayName   = "Stockpile",
    Description   = "Start each level with two extra balls",
    Cost          = 2,
    Prerequisites = { "ball.spare" },
    OnLevelStart  = ctx => ctx.BonusStartingBalls += 2,
});
```

`RunContext` (`Definitions/SkillNode.cs`) is the surface for skills to mutate
a new level — paddle width multiplier, ball speed multiplier, bonus balls,
or arbitrary world tweaks via `ctx.World`.

### Add a new chain-reaction behavior

```csharp
public sealed class BrickReflectsToParityBehavior : IBehavior {
    private IDisposable? _sub;
    public void OnAttach(Entity self, World world)
        => _sub = world.Bus.Subscribe<BrickDestroyedEvent>(e => {
            // your chain logic
        });
    public void OnDetach(Entity self, World world) => _sub?.Dispose();
}
```

Attach it via `BrickDef.BehaviorFactories`.

---

## What's in the demo content

- **Bricks** — basic (3 tiers), indestructible stone, bomb, resonant chain,
  drifter (moving).
- **Powerups** — wider paddle, bigger ball, multiball, explosive balls,
  splitter, Forge map key.
- **Maps** — Training Grounds (2 levels), Cascade Halls, The Forge (locked).
- **Skills** — paddle width tiers, ball speed, spare ball, Forge scout
  (unlock map), bargain hunter.

It's enough to demonstrate every extensibility seam. Add to taste.

---

## Known limits / TODO

- No save/load yet (`PlayerProfile` is in-memory). Add a JSON layer when ready.
- Collision is O(balls × bricks) per frame. Switch to a coarse grid once you
  exceed a few hundred bricks.
- No audio — there's no audio system wired up. MonoGame has SoundEffect /
  Song APIs; add `IAudio` and an `AudioSystem` listening on events.
- Skill tree UI is a flat list. A graph layout would be a nice upgrade.
- Powerup effects re-apply doesn't fully stack (intentional refresh-by-id);
  if you want multiplicative stacking, change `EffectStack.Add`.
- No content pipeline. All visuals are code-generated rectangles, circles,
  and a hand-rolled 3×5 pixel font. Swap in real sprites by extending
  `SpriteRenderer` and `RenderSystem`.

---

## License

Add a license file before publishing. The starter code in this repo is
intentionally unencumbered for you to adapt.
