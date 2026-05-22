using Breakforge.Core;

namespace Breakforge.Effects;

/// <summary>Scales the ball radius. Applied per-ball when collected.</summary>
public sealed class BallSizeEffect : Effect
{
    public override string Id => "ball.size";
    public float Factor { get; init; } = 1.6f;
    private float _originalRadius;

    public override void OnApply(World world)
    {
        if (Target is null) return;
        var c = Target.Get<CircleCollider>();
        _originalRadius = c.Radius;
        c.Radius = _originalRadius * Factor;
        var t = Target.Get<Transform>();
        t.Size = new(c.Radius * 2f, c.Radius * 2f);
    }

    public override void OnRemove(World world)
    {
        if (Target is null) return;
        var c = Target.Get<CircleCollider>();
        c.Radius = _originalRadius;
        var t = Target.Get<Transform>();
        t.Size = new(c.Radius * 2f, c.Radius * 2f);
    }
}
