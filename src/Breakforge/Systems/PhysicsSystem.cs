using Breakforge.Core;

namespace Breakforge.Systems;

/// <summary>
/// Applies Velocity to Transform for every alive entity that has both. Brick
/// movers update position directly (no Velocity component), so they're untouched.
/// </summary>
public sealed class PhysicsSystem : ISystem
{
    public void Update(World world, float dt)
    {
        foreach (var e in world.Entities)
        {
            if (!e.IsAlive) continue;
            var v = e.TryGet<Velocity>();
            if (v is null) continue;
            var t = e.Get<Transform>();
            t.Position += v.Value * dt;
        }
    }
}
