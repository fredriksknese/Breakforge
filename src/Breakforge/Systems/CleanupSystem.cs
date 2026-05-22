using Breakforge.Core;

namespace Breakforge.Systems;

/// <summary>
/// Removes dead entities at the end of the frame, plus ticks Lifetime components
/// so FX entities self-destruct.
/// </summary>
public sealed class CleanupSystem : ISystem
{
    public void Update(World world, float dt)
    {
        foreach (var e in world.Entities)
        {
            if (!e.IsAlive) continue;
            var life = e.TryGet<Lifetime>();
            if (life is null) continue;
            life.Remaining -= dt;
            if (life.Remaining <= 0) e.Kill();
        }
        world.Entities.RemoveAll(e => !e.IsAlive);
    }
}
