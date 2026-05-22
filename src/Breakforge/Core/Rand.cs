using System;
using Microsoft.Xna.Framework;

namespace Breakforge.Core;

public static class Rand
{
    private static Random _r = new();
    public static void Seed(int seed) => _r = new Random(seed);
    public static int Range(int minIncl, int maxExcl) => _r.Next(minIncl, maxExcl);
    public static float Float() => (float)_r.NextDouble();
    public static float RangeF(float min, float max) => min + (max - min) * (float)_r.NextDouble();
    public static int Sign() => _r.Next(2) == 0 ? -1 : 1;
    public static bool Chance(float p) => _r.NextDouble() < p;
    public static Vector2 UnitVector()
    {
        float a = RangeF(0f, MathHelper.TwoPi);
        return new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
    }
    public static T Pick<T>(System.Collections.Generic.IReadOnlyList<T> list)
        => list[_r.Next(list.Count)];
}
