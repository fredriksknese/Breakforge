namespace Breakforge.Core;

/// <summary>
/// Hot-pluggable logic attached to an entity. Behaviors are how features (powerups,
/// moving bricks, chain reactions) compose without changing entity classes.
///
/// Each frame, World ticks every behavior's OnUpdate.
/// Behaviors also subscribe to EventBus to react to game events.
/// </summary>
public interface IBehavior
{
    void OnAttach(Entity self, World world) { }
    void OnDetach(Entity self, World world) { }
    void OnUpdate(Entity self, World world, float dt) { }
    void OnDraw(Entity self, World world, Microsoft.Xna.Framework.Graphics.SpriteBatch sb) { }
}
