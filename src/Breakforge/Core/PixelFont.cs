using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Breakforge.Core;

/// <summary>
/// Tiny built-in 3x5 bitmap font. Avoids the Content Pipeline / SpriteFont
/// requirement. Glyphs encoded as 15-char strings ('#' = on, '.' = off) where
/// the string is read left-to-right, top-to-bottom (3 cols x 5 rows).
/// </summary>
public static class PixelFont
{
    public const int GlyphW = 3;
    public const int GlyphH = 5;
    public const int Tracking = 1; // pixels between chars at scale=1

    private static readonly Dictionary<char, string> _glyphs = new()
    {
        [' '] = "..." + "..." + "..." + "..." + "...",
        ['A'] = ".#." + "#.#" + "###" + "#.#" + "#.#",
        ['B'] = "##." + "#.#" + "##." + "#.#" + "##.",
        ['C'] = ".##" + "#.." + "#.." + "#.." + ".##",
        ['D'] = "##." + "#.#" + "#.#" + "#.#" + "##.",
        ['E'] = "###" + "#.." + "##." + "#.." + "###",
        ['F'] = "###" + "#.." + "##." + "#.." + "#..",
        ['G'] = ".##" + "#.." + "#.#" + "#.#" + ".##",
        ['H'] = "#.#" + "#.#" + "###" + "#.#" + "#.#",
        ['I'] = "###" + ".#." + ".#." + ".#." + "###",
        ['J'] = "..#" + "..#" + "..#" + "#.#" + ".#.",
        ['K'] = "#.#" + "#.#" + "##." + "#.#" + "#.#",
        ['L'] = "#.." + "#.." + "#.." + "#.." + "###",
        ['M'] = "#.#" + "###" + "###" + "#.#" + "#.#",
        ['N'] = "#.#" + "###" + "###" + "###" + "#.#",
        ['O'] = ".#." + "#.#" + "#.#" + "#.#" + ".#.",
        ['P'] = "##." + "#.#" + "##." + "#.." + "#..",
        ['Q'] = ".#." + "#.#" + "#.#" + "###" + ".##",
        ['R'] = "##." + "#.#" + "##." + "#.#" + "#.#",
        ['S'] = ".##" + "#.." + ".#." + "..#" + "##.",
        ['T'] = "###" + ".#." + ".#." + ".#." + ".#.",
        ['U'] = "#.#" + "#.#" + "#.#" + "#.#" + ".#.",
        ['V'] = "#.#" + "#.#" + "#.#" + "#.#" + ".#.",
        ['W'] = "#.#" + "#.#" + "###" + "###" + "#.#",
        ['X'] = "#.#" + "#.#" + ".#." + "#.#" + "#.#",
        ['Y'] = "#.#" + "#.#" + ".#." + ".#." + ".#.",
        ['Z'] = "###" + "..#" + ".#." + "#.." + "###",
        ['0'] = ".#." + "#.#" + "#.#" + "#.#" + ".#.",
        ['1'] = ".#." + "##." + ".#." + ".#." + "###",
        ['2'] = "##." + "..#" + ".#." + "#.." + "###",
        ['3'] = "##." + "..#" + ".#." + "..#" + "##.",
        ['4'] = "#.#" + "#.#" + "###" + "..#" + "..#",
        ['5'] = "###" + "#.." + "##." + "..#" + "##.",
        ['6'] = ".#." + "#.." + "##." + "#.#" + ".#.",
        ['7'] = "###" + "..#" + ".#." + ".#." + ".#.",
        ['8'] = ".#." + "#.#" + ".#." + "#.#" + ".#.",
        ['9'] = ".#." + "#.#" + ".##" + "..#" + ".#.",
        ['.'] = "..." + "..." + "..." + "..." + ".#.",
        [','] = "..." + "..." + "..." + ".#." + "#..",
        [':'] = "..." + ".#." + "..." + ".#." + "...",
        ['-'] = "..." + "..." + "###" + "..." + "...",
        ['/'] = "..#" + "..#" + ".#." + "#.." + "#..",
        ['+'] = "..." + ".#." + "###" + ".#." + "...",
        ['!'] = ".#." + ".#." + ".#." + "..." + ".#.",
        ['?'] = "##." + "..#" + ".#." + "..." + ".#.",
        ['%'] = "#.#" + "..#" + ".#." + "#.." + "#.#",
        ['='] = "..." + "###" + "..." + "###" + "...",
        ['<'] = "..#" + ".#." + "#.." + ".#." + "..#",
        ['>'] = "#.." + ".#." + "..#" + ".#." + "#..",
        ['('] = "..#" + ".#." + ".#." + ".#." + "..#",
        [')'] = "#.." + ".#." + ".#." + ".#." + "#..",
        ['*'] = "#.#" + ".#." + "###" + ".#." + "#.#",
        ['['] = "###" + "#.." + "#.." + "#.." + "###",
        [']'] = "###" + "..#" + "..#" + "..#" + "###",
        ['_'] = "..." + "..." + "..." + "..." + "###",
        ['\''] = ".#." + ".#." + "..." + "..." + "...",
    };

    public static int MeasureWidth(string text, int scale = 1)
        => text.Length == 0 ? 0 : text.Length * GlyphW * scale + (text.Length - 1) * Tracking * scale;

    public static void Draw(SpriteBatch sb, string text, Vector2 pos, Color color, int scale = 2)
    {
        text = text.ToUpperInvariant();
        int x0 = (int)pos.X;
        int y0 = (int)pos.Y;
        int cellW = (GlyphW + Tracking) * scale;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (!_glyphs.TryGetValue(c, out var g)) continue;
            for (int gy = 0; gy < GlyphH; gy++)
            for (int gx = 0; gx < GlyphW; gx++)
            {
                if (g[gy * GlyphW + gx] == '#')
                    sb.Draw(Primitives.Pixel,
                        new Rectangle(x0 + i * cellW + gx * scale, y0 + gy * scale, scale, scale),
                        color);
            }
        }
    }

    public static void DrawCentered(SpriteBatch sb, string text, Vector2 center, Color color, int scale = 2)
    {
        int w = MeasureWidth(text, scale);
        int h = GlyphH * scale;
        Draw(sb, text, new Vector2(center.X - w * 0.5f, center.Y - h * 0.5f), color, scale);
    }
}
