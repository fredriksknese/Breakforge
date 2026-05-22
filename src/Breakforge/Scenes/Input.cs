using Microsoft.Xna.Framework.Input;

namespace Breakforge.Scenes;

/// <summary>Small edge-trigger keyboard helper shared by menu scenes.</summary>
public sealed class KeyEdges
{
    private KeyboardState _prev;

    public bool Pressed(Keys k)
    {
        var now = Keyboard.GetState();
        bool down = now.IsKeyDown(k) && _prev.IsKeyUp(k);
        return down;
    }

    public void EndFrame() => _prev = Keyboard.GetState();
}
