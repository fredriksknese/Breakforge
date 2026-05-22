using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Breakforge.Scenes;

public interface IScene
{
    void Enter(SceneContext ctx) { }
    void Exit() { }
    void Update(SceneContext ctx, GameTime gt);
    void Draw(SceneContext ctx, SpriteBatch sb);
}

/// <summary>Bag of cross-scene references handed to every scene.</summary>
public sealed class SceneContext
{
    public required Definitions.Registry Registry { get; init; }
    public required Player.PlayerProfile Profile { get; init; }
    public required SceneManager Scenes { get; init; }
    public required Rectangle ViewportBounds { get; init; }
}

public sealed class SceneManager
{
    public IScene? Current { get; private set; }
    public IScene? Next { get; private set; }

    public void Goto(IScene scene) => Next = scene;

    public void TickTransitions(SceneContext ctx)
    {
        if (Next is null) return;
        Current?.Exit();
        Current = Next;
        Next = null;
        Current.Enter(ctx);
    }
}
