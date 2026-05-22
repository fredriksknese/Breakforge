using Breakforge.Core;

namespace Breakforge.Effects;

/// <summary>Multiplies the paddle width by Factor for Duration seconds.</summary>
public sealed class PaddleWidthEffect : Effect
{
    public override string Id => "paddle.width";
    public float Factor { get; init; } = 1.5f;
    private float _originalWidth;

    public override void OnApply(World world)
    {
        if (Target is null) return;
        var t = Target.Get<Transform>();
        _originalWidth = t.Size.X;
        t.Size.X = _originalWidth * Factor;
    }

    public override void OnRemove(World world)
    {
        if (Target is null) return;
        Target.Get<Transform>().Size.X = _originalWidth;
    }
}
