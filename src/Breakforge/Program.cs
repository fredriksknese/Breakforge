using System;

namespace Breakforge;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        using var game = new BreakforgeGame();
        game.Run();
    }
}
